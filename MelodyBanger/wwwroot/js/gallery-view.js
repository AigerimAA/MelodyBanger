const GalleryView = {
    isLoading: false,
    hasMore: true,
    currentPage: 1,
    itemsPerPage: 20, 

    async init() {
        this.setupInfiniteScroll();
        await this.reset();
    },

    setupInfiniteScroll() {
        window.addEventListener('scroll', () => {
            const { galleryLoader, galleryContainer } = elements;

            if (this.isLoading || !this.hasMore) return;

            const scrollPosition = window.innerHeight + window.scrollY;
            const threshold = document.body.offsetHeight - 500;

            if (scrollPosition >= threshold) {
                this.loadMore();
            }
        });
    },

    async reset() {
        this.currentPage = 1;
        this.hasMore = true;
        this.isLoading = false;

        elements.galleryContainer.innerHTML = '';

        await this.loadMore();
    },

    async loadMore() {
        if (this.isLoading || !this.hasMore) return;

        this.isLoading = true;
        this.showLoader(true);

        try {
            const data = await fetchSongs(this.currentPage, this.itemsPerPage);

            if (!data.items || data.items.length === 0) {
                this.hasMore = false;
                this.showLoader(false);
                return;
            }

            this.render(data.items);

            this.currentPage++;

            if (data.items.length < this.itemsPerPage) {
                this.hasMore = false;
            }

        } catch (error) {
            console.error('Failed to load gallery:', error);
            this.showError();
        } finally {
            this.isLoading = false;
            this.showLoader(false);
        }
    },

    render(songs) {
        songs.forEach(song => {
            const card = this.createCard(song);
            elements.galleryContainer.appendChild(card);
        });

        this.lazyLoadImages();
    },

    createCard(song) {
        const card = document.createElement('div');
        card.className = 'gallery-card';
        card.dataset.index = song.index;

        const coverUrl = `/api/songs/cover?seed=${AppState.params.seed}&index=${song.index}&title=${encodeURIComponent(song.title)}&artist=${encodeURIComponent(song.artist)}`;
        const midiUrl = `/api/songs/music?seed=${AppState.params.seed}&index=${song.index}&genre=${encodeURIComponent(song.genre)}`;

        const review = song.review || 'No review available.';
        const shortReview = review.length > 100 ? review.substring(0, 100) + '...' : review;

        card.innerHTML = `
            <img class="gallery-card-cover" 
                 src="${coverUrl}" 
                 alt="${this.escapeHtml(song.title)}"
                 loading="lazy"
                 onerror="this.style.display='none'">
            <div class="gallery-card-info">
                <div class="gallery-card-title">${this.escapeHtml(song.title)}</div>
                <div class="gallery-card-artist">
                    <i class="fas fa-user"></i> ${this.escapeHtml(song.artist)}
                </div>
                <div class="gallery-card-album">
                    <i class="fas fa-compact-disc"></i> ${this.escapeHtml(song.album)}
                </div>
                <div class="gallery-card-meta">
                    <span class="gallery-card-genre">${this.escapeHtml(song.genre)}</span>
                    <div class="gallery-card-likes">
                    <button class="like-btn-gallery" data-seed="${AppState.params.seed}" data-index="${song.index}">
                        <i class="${LikesManager.isLiked(AppState.params.seed, song.index) ? 'fas fa-heart' : 'far fa-heart'}"></i>
                    </button>
                    <span class="likes-count">${song.likes + (LikesManager.isLiked(AppState.params.seed, song.index) ? 1 : 0)}</span>
                </div>
                </div>
                <div class="gallery-card-review">
                    <i class="fas fa-quote-left"></i> ${this.escapeHtml(shortReview)}
                </div>
                <button class="play-btn gallery-play-btn" data-song-id="${song.index}" data-midi-url="${midiUrl}">
                    <i class="fas fa-play"></i> Play
                </button>
            </div>
        `;

        const playBtn = card.querySelector('.play-btn');
        playBtn.addEventListener('click', (e) => {
            e.stopPropagation();
            window.audioPlayer.toggle(song.index, midiUrl);
        });

        const likeBtn = card.querySelector('.like-btn-gallery');
        likeBtn.addEventListener('click', (e) => {
            e.stopPropagation();
            const seed = parseInt(likeBtn.dataset.seed);
            const index = parseInt(likeBtn.dataset.index);
            const isNowLiked = LikesManager.toggleLike(seed, index);

            const icon = likeBtn.querySelector('i');
            icon.className = isNowLiked ? 'fas fa-heart' : 'far fa-heart';

            const likesSpan = card.querySelector('.likes-count');
            let currentLikes = parseInt(likesSpan.textContent);
            likesSpan.textContent = isNowLiked ? currentLikes + 1 : currentLikes - 1;
        });

        card.addEventListener('click', (e) => {
            if (e.target === playBtn || playBtn.contains(e.target)) return;
            this.showSongDetails(song, coverUrl, midiUrl);
        });

        return card;
    },

    showSongDetails(song, coverUrl, midiUrl) {
        const modal = document.createElement('div');
        modal.className = 'song-modal';

        modal.innerHTML = `
            <div class="song-modal-content">
                <button class="modal-close">&times;</button>
                <div class="modal-grid">
                    <img class="modal-cover" src="${coverUrl}" alt="${this.escapeHtml(song.title)}">
                    <div class="modal-info">
                        <h2>${this.escapeHtml(song.title)}</h2>
                        <h3>${this.escapeHtml(song.artist)}</h3>
                        <p><strong>Album:</strong> ${this.escapeHtml(song.album)}</p>
                        <p><strong>Genre:</strong> ${this.escapeHtml(song.genre)}</p>
                        <p><strong>Likes:</strong> <i class="fas fa-heart" style="color:#e53e3e"></i> ${song.likes}</p>
                        <div class="modal-review">
                            <strong>Review:</strong><br>
                            "${this.escapeHtml(song.review || 'No review available.')}"
                        </div>
                        <button class="play-btn modal-play-btn" data-song-id="${song.index}" data-midi-url="${midiUrl}">
                            <i class="fas fa-play"></i> Play Full Song
                        </button>
                    </div>
                </div>
            </div>
        `;

        document.body.appendChild(modal);

        const closeBtn = modal.querySelector('.modal-close');
        closeBtn.addEventListener('click', () => modal.remove());
        modal.addEventListener('click', (e) => {
            if (e.target === modal) modal.remove();
        });

        const playBtn = modal.querySelector('.modal-play-btn');
        playBtn.addEventListener('click', () => {
            window.audioPlayer.toggle(song.index, midiUrl);
        });
    },

    showLoader(show) {
        if (show) {
            elements.galleryLoader.style.display = 'block';
        } else {
            elements.galleryLoader.style.display = 'none';
        }
    },

    showError() {
        const errorDiv = document.createElement('div');
        errorDiv.className = 'gallery-error';
        errorDiv.innerHTML = `
            <i class="fas fa-exclamation-triangle"></i>
            Failed to load more songs. 
            <button onclick="GalleryView.retry()">Retry</button>
        `;
        elements.galleryContainer.appendChild(errorDiv);
    },

    async retry() {
        const errorDiv = elements.galleryContainer.querySelector('.gallery-error');
        if (errorDiv) errorDiv.remove();
        await this.loadMore();
    },

    lazyLoadImages() {
        if ('IntersectionObserver' in window) {
            const imageObserver = new IntersectionObserver((entries, observer) => {
                entries.forEach(entry => {
                    if (entry.isIntersecting) {
                        const img = entry.target;
                        if (img.dataset.src) {
                            img.src = img.dataset.src;
                            img.removeAttribute('data-src');
                        }
                        observer.unobserve(img);
                    }
                });
            });

            document.querySelectorAll('.gallery-card-cover[data-src]').forEach(img => {
                imageObserver.observe(img);
            });
        }
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

window.loadGalleryView = async (reset = false) => {
    if (reset) {
        await GalleryView.reset();
    } else {
        await GalleryView.loadMore();
    }
};

const galleryStyles = document.createElement('style');
galleryStyles.textContent = `
    .gallery-card-meta {
        display: flex;
        justify-content: space-between;
        align-items: center;
        margin: 8px 0;
    }
    
    .gallery-card-review {
        font-size: 12px;
        color: #666;
        margin: 8px 0;
        font-style: italic;
        line-height: 1.4;
    }
    
    .gallery-play-btn {
        width: 100%;
        margin-top: 8px;
        background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
    }
    
    .gallery-error {
        text-align: center;
        padding: 40px;
        background: white;
        border-radius: 12px;
        margin: 20px;
    }
    
    .gallery-error button {
        margin-left: 10px;
        padding: 5px 10px;
        background: #667eea;
        color: white;
        border: none;
        border-radius: 6px;
        cursor: pointer;
    }
    
    /* Modal styles */
    .song-modal {
        position: fixed;
        top: 0;
        left: 0;
        width: 100%;
        height: 100%;
        background: rgba(0, 0, 0, 0.8);
        display: flex;
        justify-content: center;
        align-items: center;
        z-index: 1000;
        animation: fadeIn 0.2s ease;
    }
    
    .song-modal-content {
        background: white;
        border-radius: 16px;
        max-width: 800px;
        width: 90%;
        max-height: 90vh;
        overflow-y: auto;
        position: relative;
        animation: slideUp 0.3s ease;
    }
    
    .modal-close {
        position: absolute;
        top: 16px;
        right: 20px;
        background: none;
        border: none;
        font-size: 32px;
        cursor: pointer;
        color: #999;
        transition: color 0.2s;
        z-index: 1;
    }
    
    .modal-close:hover {
        color: #333;
    }
    
    .modal-grid {
        display: flex;
        gap: 24px;
        padding: 24px;
        flex-wrap: wrap;
    }
    
    .modal-cover {
        width: 250px;
        height: 250px;
        border-radius: 12px;
        object-fit: cover;
        box-shadow: 0 4px 6px rgba(0, 0, 0, 0.1);
    }
    
    .modal-info {
        flex: 1;
        min-width: 250px;
    }
    
    .modal-info h2 {
        margin: 0 0 8px 0;
        color: #333;
    }
    
    .modal-info h3 {
        margin: 0 0 16px 0;
        color: #667eea;
    }
    
    .modal-review {
        background: #f8f9fa;
        padding: 16px;
        border-radius: 8px;
        margin: 16px 0;
        font-style: italic;
    }
    
    .modal-play-btn {
        width: 100%;
        padding: 12px;
        font-size: 16px;
    }
    
    @keyframes fadeIn {
        from { opacity: 0; }
        to { opacity: 1; }
    }
    
    @keyframes slideUp {
        from { transform: translateY(50px); opacity: 0; }
        to { transform: translateY(0); opacity: 1; }
    }
    
    @media (max-width: 640px) {
        .modal-grid {
            flex-direction: column;
            align-items: center;
        }
        
        .modal-cover {
            width: 200px;
            height: 200px;
        }
    }
`;

document.head.appendChild(galleryStyles);