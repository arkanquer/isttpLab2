let genres = []; let games = []; let players = []; let reviews = [];
let loggedInPlayerId = null;
let isAdmin = false;

function switchTab(tabId) {
    document.querySelectorAll('.tab-content').forEach(el => el.classList.remove('active'));
    document.querySelectorAll('.tab-btn').forEach(el => el.classList.remove('active'));
    document.getElementById(tabId).classList.add('active');
    const targetBtn = Array.from(document.querySelectorAll('.tab-btn'))
        .find(btn => btn.getAttribute('onclick') && btn.getAttribute('onclick').includes(tabId));
    if (targetBtn) targetBtn.classList.add('active');
    refreshAllData();
}

function loginUser() {
    const sel = document.getElementById('global-user-select');
    if (!sel.value) return alert("Оберіть користувача!");
    loggedInPlayerId = parseInt(sel.value, 10);
    const player = players.find(p => p.id === loggedInPlayerId);
    isAdmin = player.nickname.toLowerCase() === 'admin';
    document.getElementById('auth-selector-wrapper').style.display = 'none';
    document.getElementById('auth-status-wrapper').style.display = 'inline';
    const badge = document.getElementById('role-badge');
    if (isAdmin) {
        badge.innerHTML = `Роль: <span style="color: #5a75a0;"><b>Адміністратор (${player.nickname})</b></span>`;
    } else {
        badge.innerHTML = `Роль: <span style="color: #689f7d;"><b>Користувач (${player.nickname})</b></span>`;
    }
    document.getElementById('review-lock-msg').style.display = 'none';
    document.getElementById('review-form-wrapper').style.display = 'block';
    document.getElementById('review-player-mock').value = player.nickname;
    document.getElementById('library-lock-msg').style.display = 'none';
    document.getElementById('library-content').style.display = 'block';
    refreshAllData();
}

function logoutUser() {
    loggedInPlayerId = null;
    isAdmin = false;
    document.getElementById('auth-selector-wrapper').style.display = 'inline';
    document.getElementById('auth-status-wrapper').style.display = 'none';
    document.getElementById('review-lock-msg').style.display = 'block';
    document.getElementById('review-form-wrapper').style.display = 'none';
    document.getElementById('library-lock-msg').style.display = 'block';
    document.getElementById('library-content').style.display = 'none';
    refreshAllData();
}

function refreshAllData() {
    Promise.all([
        fetch('api/genres').then(r => r.json()),
        fetch('api/games').then(r => r.json()),
        fetch('api/players').then(r => r.json()),
        fetch('api/reviews').then(r => r.json())
    ])
    .then(([genresData, gamesData, playersData, reviewsData]) => {
        genres = genresData; games = gamesData; players = playersData; reviews = reviewsData;
        renderAdminForms();
        renderGenres(); 
        renderPlayers();
        renderGames(); 
        renderMyLibrary(); 
        updateUserDropdowns();
    });
}

function renderAdminForms() {
    const gameArea = document.getElementById('game-form-area');
    const genreArea = document.getElementById('genre-form-area');
    const playerArea = document.getElementById('player-form-area');
    const adminExists = players.some(p => p.nickname.toLowerCase() === 'admin');
    document.getElementById('nav-genres').style.display = isAdmin ? 'inline-block' : 'none';
    document.getElementById('nav-players').style.display = (isAdmin || !adminExists) ? 'inline-block' : 'none';
    if (!isAdmin) {
        const activeTab = document.querySelector('.tab-content.active');
        if (activeTab && (activeTab.id === 'tab-genres' || (activeTab.id === 'tab-players' && adminExists))) {
            switchTab('tab-games');
            return;
        }
    }
    if (isAdmin) {
        gameArea.innerHTML = `
            <div class="panel">
                <h3>Додати гру в каталог</h3>
                <form action="javascript:void(0);" onsubmit="addGame()" class="form-grid">
                    <div><label>Назва гри</label><input type="text" id="game-name" required></div>
                    <div><label>Оберіть жанр</label><select id="game-genre-select" required></select></div>
                    <button type="submit" class="btn">Зберегти</button>
                </form>
            </div>`; 
        genreArea.innerHTML = `
            <div class="panel">
                <h3>Створити новий жанр</h3>
                <form action="javascript:void(0);" onsubmit="addGenre()" class="form-grid">
                    <div><label>Назва жанру</label><input type="text" id="genre-name" required></div>
                    <div><label>Опис</label><textarea id="genre-description"></textarea></div>
                    <button type="submit" class="btn">Додати</button>
                </form>
            </div>`;
    } else {
        gameArea.innerHTML = `<p class="status-msg" style="color: #e5a9a9;">Створення ігор доступне лише для Адміністратора.</p>`;
        genreArea.innerHTML = `<p class="status-msg" style="color: #e5a9a9;">Створення жанрів доступне лише для Адміністратора.</p>`;
    }

    if (isAdmin || !adminExists) {
        playerArea.innerHTML = `
            <div class="panel">
                <h3>Зареєструвати гравця ${!adminExists ? '(Створіть нікнейм Admin для керування!)' : ''}</h3>
                <form action="javascript:void(0);" onsubmit="addPlayer()" class="form-grid">
                    <div><label>Нікнейм користувача</label><input type="text" id="player-nickname" required placeholder="Введіть Admin для повних прав"></div>
                    <div><label>Email</label><input type="text" id="player-email" required></div>
                    <button type="submit" class="btn">Створити профіль</button>
                </form>
            </div>`;
    } else {
        playerArea.innerHTML = `<p class="status-msg" style="color: #e5a9a9;">Створення нових акаунтів заблоковано. Зверніться до Адміністратора.</p>`;
    }
}

function updateUserDropdowns() {
    const globalSel = document.getElementById('global-user-select');
    const revGameSel = document.getElementById('review-game-select');
    const gameGenreSel = document.getElementById('game-genre-select');
    
    if (!loggedInPlayerId) {
        globalSel.innerHTML = '<option value="">Увійти як</option>' + players.map(p => `<option value="${p.id}">${p.nickname}</option>`).join('');
    }
    if (isAdmin && gameGenreSel) {
        gameGenreSel.innerHTML = genres.map(g => `<option value="${g.id}">${g.name}</option>`).join('');
    }
    if (revGameSel) {
        revGameSel.innerHTML = '<option value="">Оберіть гру</option>' + games.map(g => `<option value="${g.id}">${g.name}</option>`).join('');
    }
}

function addGameToMyLibrary(gameId) {
    fetch(`api/players/${loggedInPlayerId}/library/${gameId}`, { method: 'POST' }).then(res => {
        if (res.ok) { alert("Гру додано!"); refreshAllData(); }
        else { res.json().then(err => alert(err.message)); }
    });
}

function renderMyLibrary() {
    const tbody = document.getElementById('my-library-table-body'); tbody.innerHTML = '';
    if (!loggedInPlayerId) return;
    const currentPlayer = players.find(p => p.id === loggedInPlayerId);
    if (!currentPlayer || !currentPlayer.library || currentPlayer.library.length === 0) {
        tbody.innerHTML = '<tr><td style="color: #a0aec0;">Ваша особиста бібліотека порожня.</td></tr>'; return;
    }
    currentPlayer.library.forEach(game => {
        tbody.innerHTML += `<tr><td><b>${game.name}</b></td></tr>`;
    });
}

function renderGames() {
    const tbody = document.getElementById('games-table-body'); tbody.innerHTML = '';
    games.forEach(game => {
        const gameReviews = reviews.filter(r => r.gameId === game.id);
        let revsHTML = `<div class="game-reviews-block"><b>Відгуки користувачів:</b><br>`;
        if (gameReviews.length === 0) revsHTML += `<small style="color: #a0aec0;">На цю гру відгуків ще немає.</small>`;
        else {
            gameReviews.forEach(r => {
                const author = players.find(p => p.id === r.playerId);
                const canDeleteReview = isAdmin || (loggedInPlayerId === r.playerId);
                const delReviewBtn = canDeleteReview ? `<button class="btn btn-danger btn-sm" style="padding:1px 5px; font-size:0.7em;" onclick="deleteReview(${r.id})">X</button>` : '';
                revsHTML += `<div class="review-row"><span class="rating-stars">${"★".repeat(r.rating)}</span> <b>${author ? author.nickname : 'Геймер'}:</b> ${r.text} ${delReviewBtn}</div>`;
            });
        }
        revsHTML += `</div>`;
        const currentPlayer = players.find(p => p.id === loggedInPlayerId);
        const inLib = currentPlayer && currentPlayer.library && currentPlayer.library.some(g => g.id === game.id);
        let libBtn = loggedInPlayerId ? (inLib ? `<button class="btn btn-secondary btn-sm" disabled>В бібліотеці</button>` : `<button class="btn btn-sm" onclick="addGameToMyLibrary(${game.id})">Додати собі</button>`) : '';
        let archiveBtn = isAdmin ? `<button class="btn btn-danger btn-sm" onclick="archiveGame(${game.id})">В архів</button>` : '';
        tbody.innerHTML += `<tr><td><b>${game.name}</b></td><td><span style="background-color: #edf2f7; color: #4a5568; padding:3px 6px;">${game.genre ? game.genre.name : 'Глобальна'}</span></td><td>${revsHTML}</td><td>${libBtn} ${archiveBtn}</td></tr>`;
    });
}

function renderGenres() { 
    const tbody = document.getElementById('genres-table-body');
    if (!tbody) return;
    tbody.innerHTML = genres.map(g => {
        const delBtn = isAdmin ? `<button class="btn btn-danger btn-sm" onclick="deleteGenre(${g.id})">Видалити</button>` : `<small style="color: #a0aec0;">Немає прав</small>`;
        return `<tr><td><b>${g.name}</b></td><td>${g.description || ''}</td><td>${delBtn}</td></tr>`;
    }).join(''); 
}

function renderPlayers() { 
    document.getElementById('players-table-body').innerHTML = players.map(p => {
        const delBtn = isAdmin ? `<button class="btn btn-danger btn-sm" onclick="deletePlayer(${p.id})">Видалити</button>` : `<small style="color: #a0aec0;">Немає прав</small>`;
        return `<tr><td>${p.id}</td><td><b>${p.nickname}</b></td><td>${p.email}</td><td>${delBtn}</td></tr>`;
    }).join(''); 
}

function addGame() {
    const body = { name: document.getElementById('game-name').value.trim(), genreId: parseInt(document.getElementById('game-genre-select').value, 10) };
    fetch('api/games', { method: 'POST', headers: { 'Content-Type': 'application/json' }, body: JSON.stringify(body) }).then(() => refreshAllData());
}

function archiveGame(id) { fetch(`api/games/${id}/archive`, { method: 'PATCH' }).then(() => refreshAllData()); }

function addGenre() { 
    fetch('api/genres', { method: 'POST', headers: { 'Content-Type': 'application/json' }, body: JSON.stringify({ name: document.getElementById('genre-name').value.trim(), description: document.getElementById('genre-description').value.trim() }) }).then(() => refreshAllData()); 
}

function deleteGenre(id) { fetch(`api/genres/${id}`, { method: 'DELETE' }).then(() => refreshAllData()); }

function addPlayer() {
    fetch('api/players', { method: 'POST', headers: { 'Content-Type': 'application/json' }, body: JSON.stringify({ nickname: document.getElementById('player-nickname').value.trim(), email: document.getElementById('player-email').value.trim() }) }).then(() => refreshAllData()); 
}

function deletePlayer(id) { fetch(`api/players/${id}`, { method: 'DELETE' }).then(() => refreshAllData()); }
function deleteReview(id) { fetch(`api/reviews/${id}`, { method: 'DELETE' }).then(() => refreshAllData()); }

function addReview() {
    const body = { playerId: loggedInPlayerId, gameId: parseInt(document.getElementById('review-game-select').value, 10), rating: parseInt(document.getElementById('review-rating').value, 10), text: document.getElementById('review-text').value.trim() };
    fetch('api/reviews', { method: 'POST', headers: { 'Content-Type': 'application/json' }, body: JSON.stringify(body) }).then(res => {
        if (res.ok) { document.getElementById('review-text').value = ''; switchTab('tab-games'); }
        else { res.json().then(err => alert(err.message)); }
    });
}

refreshAllData();