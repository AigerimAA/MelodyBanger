class AudioPlayer {
    constructor() {
        this.isPlaying = false;
        this.currentSongId = null;
        this.synths = [];
        this.progressInterval = null;
        this.currentDuration = 0;
    }

    async play(songId, midiUrl) {
        await this.stop();
        this.currentSongId = songId;

        try {
            await Tone.start();

            const response = await fetch(midiUrl);
            if (!response.ok) throw new Error(`HTTP ${response.status}`);

            const arrayBuffer = await response.arrayBuffer();
            const midi = new Midi(arrayBuffer);

            if (!midi.tracks || midi.tracks.length === 0) {
                throw new Error('MIDI file is empty or corrupted');
            }

            this.currentDuration = midi.duration;
            this.resetProgressBars(songId);
            this.startProgressTracking(songId);

            const now = Tone.now() + 0.1;
            this.synths = [];

            midi.tracks.forEach((track, trackIndex) => {
                let synth;

                if (trackIndex === 0) {
                    synth = new Tone.PolySynth(Tone.Synth, {
                        oscillator: { type: 'triangle' },
                        envelope: { attack: 0.02, decay: 0.1, sustain: 0.3, release: 0.8 }
                    }).toDestination();
                    synth.volume.value = -6;
                } else if (trackIndex === 1) {
                    synth = new Tone.PolySynth(Tone.Synth, {
                        oscillator: { type: 'sine' },
                        envelope: { attack: 0.05, decay: 0.2, sustain: 0.4, release: 1.0 }
                    }).toDestination();
                    synth.volume.value = -12;
                } else {
                    synth = new Tone.PolySynth(Tone.Synth, {
                        oscillator: { type: 'sawtooth' },
                        envelope: { attack: 0.05, decay: 0.1, sustain: 0.5, release: 0.5 }
                    }).toDestination();
                    synth.volume.value = -10;
                }

                this.synths.push(synth);

                track.notes.forEach(note => {
                    synth.triggerAttackRelease(
                        note.name,
                        note.duration,
                        now + note.time,
                        note.velocity
                    );
                });
            });

            this.isPlaying = true;
            this.updatePlayButton(songId, true);

            setTimeout(() => {
                if (this.currentSongId === songId) {
                    this.stop();
                }
            }, (this.currentDuration + 0.5) * 1000);

        } catch (error) {
            console.error('Playback failed:', error);
            this.playWithWebAudioFallback();
        }
    }

    startProgressTracking(songId) {
        const startTime = Date.now();

        if (this.progressInterval) {
            clearInterval(this.progressInterval);
        }

        this.progressInterval = setInterval(() => {
            if (!this.isPlaying || this.currentSongId !== songId) {
                return;
            }

            const elapsed = (Date.now() - startTime) / 1000;
            const progress = Math.min(1, elapsed / this.currentDuration);

            this.updateProgressBars(songId, progress);
            this.updateDurationDisplay(songId, elapsed);

            if (progress >= 1) {
                clearInterval(this.progressInterval);
                this.progressInterval = null;
            }
        }, 100);

    }

    resetProgressBars(songId) {
        const trackDiv = document.querySelector(`.expanded-player[data-song-id="${songId}"]`);
        if (trackDiv) {
            const progressBar = trackDiv.querySelector('.volume-track');
            if (progressBar) {
                progressBar.style.width = '0%';
            }
            const durationSpan = trackDiv.querySelector('.duration');
            if (durationSpan) {
                durationSpan.textContent = '0:00';
            }
        }
    }

    updateProgressBars(songId, progress) {
        console.log('Updating progress for song:', songId, 'progress:', progress);
        const trackDiv = document.querySelector(`.expanded-player[data-song-id="${songId}"]`);
        console.log('Found trackDiv:', trackDiv);
        if (trackDiv) {
            const progressBar = trackDiv.querySelector('.volume-track');
            console.log('Found progressBar:', progressBar);
            if (progressBar) {
                progressBar.style.width = `${progress * 100}%`;
            }
        }
    }

    updateDurationDisplay(songId, elapsed) {
        const trackDiv = document.querySelector(`.expanded-player[data-song-id="${songId}"]`);
        if (trackDiv) {
            const durationSpan = trackDiv.querySelector('.duration');
            if (durationSpan) {
                const remaining = Math.max(0, this.currentDuration - elapsed);
                const minutes = Math.floor(remaining / 60);
                const seconds = Math.floor(remaining % 60);
                durationSpan.textContent = `${minutes}:${seconds.toString().padStart(2, '0')}`;
            }
        }
    }

    async stop() {
        if (this.progressInterval) {
            clearInterval(this.progressInterval);
            this.progressInterval = null;
        }

        this.synths.forEach(synth => {
            try {
                synth.releaseAll();
                synth.dispose();
            } catch (e) {
                console.log('Error disposing synth', e);
            }
        });
        this.synths = [];

        Tone.Transport.stop();
        Tone.Transport.cancel();

        if (this.currentSongId) {
            this.resetProgressBars(this.currentSongId);
            this.updatePlayButton(this.currentSongId, false);
        }

        this.isPlaying = false;
        this.currentSongId = null;
        this.currentDuration = 0;
    }

    updatePlayButton(songId, isPlaying) {
        const buttons = document.querySelectorAll(`.play-expanded-btn[data-song-id="${songId}"]`);
        buttons.forEach(btn => {
            if (isPlaying) {
                btn.innerHTML = `<i class="fas fa-pause"></i>`;
                btn.classList.add('playing');
            } else {
                btn.innerHTML = `<i class="fas fa-play"></i>`;
                btn.classList.remove('playing');
            }
        });

        document.querySelectorAll(`.play-btn[data-song-id="${songId}"]`).forEach(btn => {
            btn.innerHTML = isPlaying
                ? '<i class="fas fa-stop"></i> Stop'
                : '<i class="fas fa-play"></i> Play';
            btn.classList.toggle('playing', isPlaying);
        });
    }

    async toggle(songId, midiUrl) {
        if (this.isPlaying && this.currentSongId === songId) {
            await this.stop();
        } else {
            await this.play(songId, midiUrl);
        }
    }

    playWithWebAudioFallback() {
        const AudioContext = window.AudioContext || window.webkitAudioContext;
        if (!AudioContext) return;

        const audioCtx = new AudioContext();
        const osc = audioCtx.createOscillator();
        const gain = audioCtx.createGain();
        osc.connect(gain);
        gain.connect(audioCtx.destination);

        osc.frequency.value = 440;
        gain.gain.value = 0.1;
        osc.start();
        gain.gain.exponentialRampToValueAtTime(0.00001, audioCtx.currentTime + 1);
        osc.stop(audioCtx.currentTime + 1);

        this.isPlaying = true;
        this.updatePlayButton(this.currentSongId, true);

        let elapsed = 0;
        const interval = setInterval(() => {
            elapsed += 0.1;
            this.updateProgressBars(this.currentSongId, elapsed / 2);
            if (elapsed >= 2) {
                clearInterval(interval);
                this.isPlaying = false;
                this.updatePlayButton(this.currentSongId, false);
                audioCtx.close();
            }
        }, 100);
    }

}

window.audioPlayer = new AudioPlayer();