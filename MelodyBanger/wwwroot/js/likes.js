const LikesManager = {
    STORAGE_KEY: 'melodybanger_likes',

    getLikes() {
        const stored = localStorage.getItem(this.STORAGE_KEY);
        if (!stored) return {};
        try {
            return JSON.parse(stored);
        } catch {
            return {};
        }
    },

    saveLikes(likes) {
        localStorage.setItem(this.STORAGE_KEY, JSON.stringify(likes));
    },

    getLikeKey(seed, songIndex) {
        return `${seed}_${songIndex}`;
    },

    isLiked(seed, songIndex) {
        const likes = this.getLikes();
        const key = this.getLikeKey(seed, songIndex);
        return likes[key] === true;
    },

    toggleLike(seed, songIndex) {
        const likes = this.getLikes();
        const key = this.getLikeKey(seed, songIndex);

        if (likes[key]) {
            delete likes[key];
        } else {
            likes[key] = true;
        }

        this.saveLikes(likes);
        return likes[key] === true;
    },

    getLikedCount(seed, songIndex) {
        return this.isLiked(seed, songIndex) ? 1 : 0;
    }
};

window.LikesManager = LikesManager;