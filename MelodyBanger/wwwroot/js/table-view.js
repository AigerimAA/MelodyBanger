const TableView = {
    async load(page) {
        const { tableBody, pageInfo, prevPageBtn, nextPageBtn } = elements;

        tableBody.innerHTML = '<tr><td colspan="7" class="loading">Loading...</td></tr>';

        try {
            const data = await fetchSongs(page);

            if (!data.items || data.items.length === 0) {
                tableBody.innerHTML = '<tr><td colspan="7">No songs found</td></tr>';
                return;
            }

            this.render(data.items);

            pageInfo.textContent = `Page ${data.currentPage}`;
            prevPageBtn.disabled = data.currentPage <= 1;
            nextPageBtn.disabled = data.currentPage >= data.totalPages;

        } catch (error) {
            console.error('Failed to load table view:', error);
            tableBody.innerHTML = '<tr><td colspan="7">Error loading songs</td></tr>';
        }
    },

    render(songs) {
        const { tableBody } = elements;
        tableBody.innerHTML = '';

        songs.forEach((song, idx) => {
            const row = tableBody.insertRow();
            row.dataset.index = song.index;

            row.innerHTML = `
                <td>${song.index}</td>
                <td class="song-title">${this.escapeHtml(song.title)}</td>
                <td>${this.escapeHtml(song.artist)}</td>
                <td>${this.escapeHtml(song.album)}</td>
                <td><span class="genre-badge">${this.escapeHtml(song.genre)}</span></td>
                <td class="likes-cell">
                    <button class="like-btn" data-seed="${AppState.params.seed}" data-index="${song.index}">
                        <i class="${LikesManager.isLiked(AppState.params.seed, song.index) ? 'fas fa-heart' : 'far fa-heart'}"></i>
                    </button>
                    <span class="likes-count">${song.likes + (LikesManager.isLiked(AppState.params.seed, song.index) ? 1 : 0)}</span>
                </td>
                <td>
                    <button class="expand-btn" data-index="${song.index}">
                        <i class="fas fa-chevron-down"></i>
                    </button>
                </td>
            `;

            const expandBtn = row.querySelector('.expand-btn');
            expandBtn.addEventListener('click', () => this.toggleExpand(row, song));

            const likeBtn = row.querySelector('.like-btn');
            likeBtn.addEventListener('click', (e) => {
                e.stopPropagation();
                const seed = parseInt(likeBtn.dataset.seed);
                const index = parseInt(likeBtn.dataset.index);
                const isNowLiked = LikesManager.toggleLike(seed, index);

                const icon = likeBtn.querySelector('i');
                icon.className = isNowLiked ? 'fas fa-heart' : 'far fa-heart';

                const likesSpan = row.querySelector('.likes-count');
                let currentLikes = parseInt(likesSpan.textContent);
                if (isNowLiked) {
                    likesSpan.textContent = currentLikes + 1;
                } else {
                    likesSpan.textContent = currentLikes - 1;
                }
            });

            row.addEventListener('click', (e) => {
                if (e.target.classList.contains('expand-btn') ||
                    e.target.closest('.expand-btn') ||
                    e.target.classList.contains('like-btn') ||
                    e.target.closest('.like-btn')) {
                    return;
                }
                this.toggleExpand(row, song);
            });
        });
    },

    async toggleExpand(row, song) {
        const nextRow = row.nextElementSibling;
        if (nextRow && nextRow.classList.contains('expanded-row')) {
            nextRow.remove();
            return;
        }

        const expandedRow = document.createElement('tr');
        expandedRow.classList.add('expanded-row');

        const coverUrl = `/api/songs/cover?seed=${AppState.params.seed}&index=${song.index}&title=${encodeURIComponent(song.title)}&artist=${encodeURIComponent(song.artist)}`;
        const midiUrl = `/api/songs/music?seed=${AppState.params.seed}&index=${song.index}&genre=${encodeURIComponent(song.genre)}`;
        const review = song.review || 'No review available.';

        expandedRow.innerHTML = `
        <td colspan="7">
            <div class="expanded-content">
                <div class="expanded-left">
                    <img class="expanded-cover" src="${coverUrl}" alt="Cover">
                    <div class="expanded-meta">
                        <div class="expanded-title">${this.escapeHtml(song.title)}</div>
                        <div class="expanded-artist-info">
                            from <strong>${this.escapeHtml(song.album)}</strong> by <strong>${this.escapeHtml(song.artist)}</strong>
                        </div>
                        <div class="label">404 Records, 2026</div>
                    </div>
                </div>
    
                <div class="expanded-right">
                    <div class="expanded-player" data-song-id="${song.index}">
                        <button class="play-expanded-btn" data-song-id="${song.index}" data-midi-url="${midiUrl}">
                            <i class="fas fa-play"></i>
                        </button>

                        <i class="fas fa-volume-up volume-icon"></i>

                        <div class="player-controls">
                            <div class="volume-slider">
                                <div class="volume-track"></div>
                            </div>
                            <span class="duration">0:00</span>
                        </div>
                    </div>
        
                    <div class="lyrics">
                        <strong>Lyrics</strong>
                        <pre>${this.escapeHtml(song.lyrics || 'No lyrics available.')}</pre>
                    </div>
                </div>
            </div>
        </td>
    `;

        row.parentNode.insertBefore(expandedRow, row.nextSibling);

        const playBtn = expandedRow.querySelector('.play-expanded-btn');
        playBtn.addEventListener('click', () => {
            window.audioPlayer.toggle(song.index, midiUrl);
        });
    },

    escapeHtml(str) {
        if (!str) return '';
        return str
            .replace(/&/g, '&amp;')
            .replace(/</g, '&lt;')
            .replace(/>/g, '&gt;')
            .replace(/"/g, '&quot;')
            .replace(/'/g, '&#39;');
    }
};

window.loadTableView = () => TableView.load(AppState.currentPage);

const style = document.createElement('style');
style.textContent = `
    .genre-badge {
        background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
        color: white;
        padding: 4px 8px;
        border-radius: 12px;
        font-size: 12px;
    }
    .likes-cell {
        color: #e53e3e;
        font-weight: bold;
    }
    .song-title {
        font-weight: 600;
        color: #333;
    }
    .play-btn.playing {
        background: #e53e3e;
    }
`;
document.head.appendChild(style);