const API_BASE = 'https://localhost:7043/api';

// DOM Elements
const moviesContainer = document.getElementById('moviesContainer');
const ticketsContainer = document.getElementById('ticketsContainer');
const loading = document.getElementById('loading');
const errorState = document.getElementById('error');
const authBtn = document.getElementById('authBtn');
const authModal = document.getElementById('authModal');
const showtimesModal = document.getElementById('showtimesModal');
const authForm = document.getElementById('authForm');
const toggleAuthMode = document.getElementById('toggleAuthMode');
const modalTitle = document.getElementById('modalTitle');
const dashboardLink = document.getElementById('dashboardLink');
const dashboardSection = document.getElementById('dashboard');
const moviesSection = document.getElementById('movies');

let isLoginMode = true;
let token = localStorage.getItem('cinema_token');

// Initialize
document.addEventListener('DOMContentLoaded', () => {
    fetchMovies();
    updateAuthUI();
});

// Utility: Show Toast
function showToast(message, isError = false) {
    const toast = document.getElementById('toast');
    toast.textContent = message;
    toast.style.background = isError ? 'var(--accent)' : '#2e7d32';
    toast.classList.remove('hidden');
    toast.style.opacity = '1';
    
    setTimeout(() => {
        toast.style.opacity = '0';
        setTimeout(() => toast.classList.add('hidden'), 300);
    }, 3000);
}

// Fetch Movies
async function fetchMovies() {
    try {
        const response = await fetch(`${API_BASE}/movies`);
        if (!response.ok) throw new Error('Failed to fetch');
        
        const movies = await response.json();
        renderMovies(movies);
    } catch (err) {
        console.error('Error fetching movies:', err);
        loading.classList.add('hidden');
        errorState.classList.remove('hidden');
    }
}

// Render Movies
function renderMovies(movies) {
    loading.classList.add('hidden');
    moviesContainer.classList.remove('hidden');
    
    moviesContainer.innerHTML = movies.map(movie => `
        <div class="movie-card">
            <div class="movie-poster">
                <h4>${movie.title}</h4>
            </div>
            <div class="movie-details">
                <div class="movie-meta">
                    <span>🎬 ${movie.genre}</span>
                    <span>⏱ ${movie.durationInMinutes} min</span>
                </div>
                <p class="movie-desc">${movie.description}</p>
                <button class="primary-btn glow-btn w-100" onclick="viewShowtimes(${movie.id}, '${movie.title}')">View Showtimes</button>
            </div>
        </div>
    `).join('');
}

// Fetch & Show Showtimes
async function viewShowtimes(movieId, title) {
    document.getElementById('showtimesTitle').textContent = `Showtimes for ${title}`;
    const list = document.getElementById('showtimesList');
    list.innerHTML = '<p style="color:var(--text-muted)">Loading...</p>';
    showtimesModal.classList.add('visible');

    try {
        const response = await fetch(`${API_BASE}/ShowTimes/movie/${movieId}`);
        if (!response.ok) throw new Error('Failed to fetch showtimes');
        
        const showtimes = await response.json();
        
        if (showtimes.length === 0) {
            list.innerHTML = '<p style="color:var(--text-muted)">No active showtimes available currently.</p>';
            return;
        }

        list.innerHTML = showtimes.map(st => `
            <div class="showtime-card">
                <div>
                    <strong>🕒 ${new Date(st.startTime).toLocaleString()}</strong>
                    <p style="font-size: 0.8rem; color:var(--text-muted)">Ends: ${new Date(st.endTime).toLocaleTimeString()}</p>
                    <p style="color:var(--text-muted)">Tickets Left: <span style="color:#fff">${st.availableSeats}</span> | Price: $${st.price}</p>
                </div>
                <div>
                    ${token 
                        ? `<button class="primary-btn glow-btn" style="padding: 10px 20px" onclick="bookTicket(${st.id}, ${st.availableSeats})">Book Now</button>`
                        : `<button class="premium-btn" onclick="promptLogin()">Login to Book</button>`
                    }
                </div>
            </div>
        `).join('');

    } catch (err) {
        list.innerHTML = `<p style="color:var(--accent)">${err.message}</p>`;
    }
}

function promptLogin() {
    showtimesModal.classList.remove('visible');
    authModal.classList.add('visible');
}

// Book Ticket
async function bookTicket(showTimeId, seatsAvailable) {
    if(!token) return promptLogin();
    if(seatsAvailable <= 0) return showToast('No seats available!', true);
    
    const seatNumber = Math.floor(Math.random() * 50) + 1; // Assign random seat for demo purposes

    try {
        const response = await fetch(`${API_BASE}/Tickets`, {
            method: 'POST',
            headers: { 
                'Content-Type': 'application/json',
                'Authorization': `Bearer ${token}` 
            },
            body: JSON.stringify({ showTimeId, seatNumber })
        });

        if (!response.ok) throw new Error('Booking failed. Check your authentication.');
        
        showToast('Ticket Booked Successfully! View it in your Dashboard.');
        showtimesModal.classList.remove('visible');
        if(dashboardSection.classList.contains('hidden')) {
            loadDashboard(); // Load if they want to click
        }
    } catch(err) {
        showToast(err.message, true);
    }
}

// Fetch My Tickets
async function loadDashboard() {
    moviesSection.classList.add('hidden');
    dashboardSection.classList.remove('hidden');
    ticketsContainer.innerHTML = '<p>Loading your tickets...</p>';

    try {
        const response = await fetch(`${API_BASE}/Tickets/my-tickets`, {
            headers: { 'Authorization': `Bearer ${token}` }
        });
        
        if (!response.ok) throw new Error('Failed to load tickets');
        
        const tickets = await response.json();
        
        if (tickets.length === 0) {
            ticketsContainer.innerHTML = '<p style="grid-column: 1/-1; text-align: center; color: var(--text-muted)">You have no booked tickets.</p>';
            return;
        }

        ticketsContainer.innerHTML = tickets.map(t => `
            <div class="movie-card">
                <div class="movie-poster" style="height: 140px; background: var(--surface-light)">
                    <h4>Ticket #${t.id}</h4>
                </div>
                <div class="movie-details" style="padding-top: 24px;">
                    <div class="movie-meta">
                        <span>💺 Seat ${t.seatNumber}</span>
                        <span style="color:${t.status === 1 ? 'inherit' : 'var(--accent)'}">${t.status === 0 ? 'Active' : 'Cancelled'}</span>
                    </div>
                    <p class="movie-desc">Booked on: ${new Date(t.bookingDate).toLocaleDateString()}</p>
                    ${t.status === 0 
                      ? `<button class="premium-btn w-100" style="color:var(--accent); border-color:var(--accent)" onclick="cancelTicket(${t.id})">Cancel Ticket</button>`
                      : ''
                    }
                </div>
            </div>
        `).join('');

    } catch (err) {
         ticketsContainer.innerHTML = `<p style="color:var(--accent)">Error: ${err.message}</p>`;
    }
}

// Cancel Ticket
async function cancelTicket(ticketId) {
    if(!confirm("Are you sure you want to cancel this ticket?")) return;

    try {
        const response = await fetch(`${API_BASE}/Tickets/${ticketId}/cancel`, {
            method: 'POST',
            headers: { 'Authorization': `Bearer ${token}` }
        });

        if (!response.ok) throw new Error('Cancellation failed');
        
        showToast('Ticket cancelled successfully');
        loadDashboard(); // Refresh
    } catch(err) {
        showToast(err.message, true);
    }
}


// Auth UI Logic
function updateAuthUI() {
    if (token) {
        authBtn.textContent = 'Logout';
        authBtn.classList.remove('primary-btn');
        authBtn.classList.add('premium-btn');
        dashboardLink.classList.remove('hidden');
    } else {
        authBtn.textContent = 'Login / Register';
        authBtn.classList.add('premium-btn');
        authBtn.classList.remove('primary-btn');
        dashboardLink.classList.add('hidden');
        moviesSection.classList.remove('hidden');
        dashboardSection.classList.add('hidden');
    }
}

// Navigation Listeners
document.querySelector('a[href="#movies"]').addEventListener('click', (e) => {
    e.preventDefault();
    dashboardSection.classList.add('hidden');
    moviesSection.classList.remove('hidden');
});

dashboardLink.addEventListener('click', (e) => {
    e.preventDefault();
    if(token) loadDashboard();
});

// Event Listeners
authBtn.addEventListener('click', () => {
    if (token) {
        if(confirm("Do you want to logout?")) {
            localStorage.removeItem('cinema_token');
            token = null;
            updateAuthUI();
        }
    } else {
        authModal.classList.add('visible');
    }
});

toggleAuthMode.addEventListener('click', () => {
    isLoginMode = !isLoginMode;
    if (isLoginMode) {
        modalTitle.textContent = 'Welcome Back';
        toggleAuthMode.textContent = 'Register';
    } else {
        modalTitle.textContent = 'Create Account';
        toggleAuthMode.textContent = 'Login';
    }
});

// Auth Submit
authForm.addEventListener('submit', async (e) => {
    e.preventDefault();
    const endpoint = isLoginMode ? '/auth/login' : '/auth/register';
    const payload = {
        email: emailInput.value,
        password: passwordInput.value,
        fullName: isLoginMode ? undefined : 'New User',
        role: 'User'
    };

    try {
        document.getElementById('authError').classList.add('hidden');
        const response = await fetch(`${API_BASE}${endpoint}`, {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify(payload)
        });

        if (!response.ok) throw new Error(await response.text() || 'Authentication failed');

        const data = await response.json();
        if(data.token) {
            token = data.token;
            localStorage.setItem('cinema_token', token);
            updateAuthUI();
            authModal.classList.remove('visible');
            authForm.reset();
            showToast('Authentication Successful');
        } else {
            showToast('Registration successful! Please login.');
            isLoginMode = true;
            toggleAuthMode.click(); toggleAuthMode.click(); // Hacky force switch
        }
    } catch (err) {
        const errBlock = document.getElementById('authError');
        errBlock.textContent = err.message;
        errBlock.classList.remove('hidden');
    }
});
