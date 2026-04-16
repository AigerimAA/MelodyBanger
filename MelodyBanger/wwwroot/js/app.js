const AppState = {
    currentView: 'table',
    currentPage: 1,
    galleryPage: 1,
    isLoading: false,
    hasMore: true,
    params: {
        seed: 123456789,
        lang: 'en',
        likes: 0,
        pageSize: 10
    },
    songsCache: new Map()
};

const elements = {
    languageSelect: document.getElementById('languageSelect'),
    seedInput: document.getElementById('seedInput'),
    randomSeedBtn: document.getElementById('randomSeedBtn'),
    likesInput: document.getElementById('likesInput'),
    likesValue: document.getElementById('likesValue'),
    tableViewBtn: document.getElementById('tableViewBtn'),
    galleryViewBtn: document.getElementById('galleryViewBtn'),
    tableView: document.getElementById('tableView'),
    galleryView: document.getElementById('galleryView'),
    prevPageBtn: document.getElementById('prevPageBtn'),
    nextPageBtn: document.getElementById('nextPageBtn'),
    pageInfo: document.getElementById('pageInfo'),
    tableBody: document.getElementById('tableBody'),
    galleryContainer: document.getElementById('galleryContainer'),
    galleryLoader: document.getElementById('galleryLoader')
};

async function init() {
    bindEvents();
    await loadData();
}

function bindEvents() {
    elements.languageSelect.addEventListener('change', () => {
        AppState.params.lang = elements.languageSelect.value;
        resetAndReload();
    });

    elements.seedInput.addEventListener('change', () => {
        AppState.params.seed = parseInt(elements.seedInput.value) || 0;
        resetAndReload();
    });

    elements.randomSeedBtn.addEventListener('click', () => {
        const randomSeed = Math.floor(Math.random() * 1000000000);
        elements.seedInput.value = randomSeed;
        AppState.params.seed = randomSeed;
        resetAndReload();
    });

    elements.likesInput.addEventListener('input', (e) => {
        const value = parseFloat(e.target.value);
        if (!isNaN(value)) {
            AppState.params.likes = Math.min(10, Math.max(0, value));
            elements.likesValue.textContent = AppState.params.likes.toFixed(1);

            elements.likesInput.classList.add('glowing');
            setTimeout(() => elements.likesInput.classList.remove('glowing'), 300);

            reloadCurrentView();
        }
    });

    elements.tableViewBtn.addEventListener('click', () => switchView('table'));
    elements.galleryViewBtn.addEventListener('click', () => switchView('gallery'));

    elements.prevPageBtn.addEventListener('click', () => {
        if (AppState.currentPage > 1) {
            AppState.currentPage--;
            loadTableView();
        }
    });

    elements.nextPageBtn.addEventListener('click', () => {
        AppState.currentPage++;
        loadTableView();
    });
}

function resetAndReload() {
    AppState.currentPage = 1;
    AppState.galleryPage = 1;
    AppState.hasMore = true;
    AppState.songsCache.clear();

    if (AppState.currentView === 'table') {
        loadTableView();
    } else {
        loadGalleryView(true);
    }
}

function switchView(view) {
    AppState.currentView = view;

    elements.tableViewBtn.classList.toggle('active', view === 'table');
    elements.galleryViewBtn.classList.toggle('active', view === 'gallery');
    elements.tableView.classList.toggle('active', view === 'table');
    elements.galleryView.classList.toggle('active', view === 'gallery');

    if (view === 'table') {
        loadTableView();
    } else {
        if (!window.galleryInitialized) {
            GalleryView.init();
            window.galleryInitialized = true;
        } else {
            GalleryView.reset();
        }
    }
}

function reloadCurrentView() {
    if (AppState.currentView === 'table') {
        loadTableView();
    } else {
        loadGalleryView(true);
    }
}

async function fetchSongs(page, pageSize = 10) {
    const params = new URLSearchParams({
        seed: AppState.params.seed,
        lang: AppState.params.lang,
        likes: AppState.params.likes,
        page: page,
        pageSize: pageSize
    });

    const url = `/api/songs?${params}&t=${Date.now()}`;

    const response = await fetch(url, {
        cache: 'no-store',
        headers: { 'Cache-Control': 'no-cache' }
    });

    if (!response.ok) throw new Error('Failed to fetch songs');
    return await response.json();
}

async function loadData() {
    await loadTableView();
}

window.AppState = AppState;
window.elements = elements;
window.fetchSongs = fetchSongs;
window.resetAndReload = resetAndReload;
window.reloadCurrentView = reloadCurrentView;
window.loadGalleryView = loadGalleryView;

if (document.readyState === 'loading') {
    document.addEventListener('DOMContentLoaded', init);
} else {
    init();
}