// ERP SaaS Application - Frontend JavaScript

const API_BASE = '/api/v1';
let authToken = null;
let currentUser = null;

// Initialize on page load
document.addEventListener('DOMContentLoaded', () => {
    // Check for stored token
    const storedToken = localStorage.getItem('authToken');
    if (storedToken) {
        authToken = storedToken;
        validateAndLoadUser();
    }
});

// Authentication Functions
function showLoginModal() {
    document.getElementById('login-modal').classList.remove('hidden');
    document.getElementById('login-error').classList.add('hidden');
}

function hideLoginModal() {
    document.getElementById('login-modal').classList.add('hidden');
}

async function handleLogin(event) {
    event.preventDefault();

    const username = document.getElementById('username').value;
    const password = document.getElementById('password').value;
    const errorDiv = document.getElementById('login-error');
    const submitBtn = document.getElementById('login-submit');

    submitBtn.disabled = true;
    submitBtn.textContent = 'Logging in...';
    errorDiv.classList.add('hidden');

    try {
        const response = await fetch(`${API_BASE}/authentication/login`, {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
            },
            body: JSON.stringify({
                userNameOrEmail: username,
                password: password
            })
        });

        if (!response.ok) {
            const errorData = await response.json().catch(() => ({}));
            throw new Error(errorData.message || 'Invalid username or password');
        }

        const data = await response.json();
        authToken = data.accessToken;
        currentUser = data.user;

        // Store token
        localStorage.setItem('authToken', authToken);

        hideLoginModal();
        updateUIForLoggedInUser();

        // Load default content
        loadContent('dashboard/executive');

    } catch (error) {
        errorDiv.textContent = error.message;
        errorDiv.classList.remove('hidden');
    } finally {
        submitBtn.disabled = false;
        submitBtn.textContent = 'Login';
    }
}

async function validateAndLoadUser() {
    try {
        const response = await fetch(`${API_BASE}/authentication/me`, {
            headers: {
                'Authorization': `Bearer ${authToken}`
            }
        });

        if (!response.ok) {
            throw new Error('Token invalid');
        }

        currentUser = await response.json();
        updateUIForLoggedInUser();

    } catch (error) {
        // Token invalid, clear it
        localStorage.removeItem('authToken');
        authToken = null;
        currentUser = null;
    }
}

function logout() {
    // Call logout endpoint
    if (authToken) {
        fetch(`${API_BASE}/authentication/logout`, {
            method: 'POST',
            headers: {
                'Authorization': `Bearer ${authToken}`
            }
        }).catch(() => {});
    }

    localStorage.removeItem('authToken');
    authToken = null;
    currentUser = null;

    updateUIForLoggedOutUser();
}

function updateUIForLoggedInUser() {
    document.getElementById('login-btn').classList.add('hidden');
    document.getElementById('logout-btn').classList.remove('hidden');
    document.getElementById('user-info').classList.remove('hidden');
    document.getElementById('user-info').textContent = `Welcome, ${currentUser?.fullName || currentUser?.userName || 'User'}`;
    document.getElementById('sidebar').classList.remove('hidden');
    document.getElementById('welcome-section').classList.add('hidden');
    document.getElementById('content-section').classList.remove('hidden');
}

function updateUIForLoggedOutUser() {
    document.getElementById('login-btn').classList.remove('hidden');
    document.getElementById('logout-btn').classList.add('hidden');
    document.getElementById('user-info').classList.add('hidden');
    document.getElementById('sidebar').classList.add('hidden');
    document.getElementById('welcome-section').classList.remove('hidden');
    document.getElementById('content-section').classList.add('hidden');
}

// Content Loading Functions
async function loadContent(endpoint) {
    const contentArea = document.getElementById('content-area');
    showLoading();

    try {
        const response = await fetch(`${API_BASE}/${endpoint}`, {
            headers: {
                'Authorization': `Bearer ${authToken}`,
                'Content-Type': 'application/json'
            }
        });

        if (!response.ok) {
            if (response.status === 401) {
                logout();
                throw new Error('Session expired. Please login again.');
            }
            throw new Error(`Error loading data: ${response.statusText}`);
        }

        const data = await response.json();
        renderContent(endpoint, data);

    } catch (error) {
        contentArea.innerHTML = `
            <div class="bg-red-50 border border-red-200 text-red-700 px-4 py-3 rounded-lg">
                <strong>Error:</strong> ${error.message}
            </div>
        `;
    } finally {
        hideLoading();
    }
}

function renderContent(endpoint, data) {
    const contentArea = document.getElementById('content-area');
    const title = getPageTitle(endpoint);

    // Handle different endpoints
    if (endpoint.startsWith('dashboard/')) {
        renderDashboard(contentArea, title, data);
    } else if (endpoint.startsWith('reports/')) {
        renderReport(contentArea, title, data);
    } else if (Array.isArray(data)) {
        renderList(contentArea, title, endpoint, data);
    } else if (data.items && Array.isArray(data.items)) {
        renderList(contentArea, title, endpoint, data.items, data);
    } else {
        renderDetail(contentArea, title, data);
    }
}

function getPageTitle(endpoint) {
    const titles = {
        'dashboard/executive': 'Executive Dashboard',
        'dashboard/financial': 'Financial Dashboard',
        'dashboard/projects': 'Project Dashboard',
        'clients': 'Clients',
        'contacts': 'Contacts',
        'projects': 'Projects',
        'contracts': 'Contracts',
        'employees': 'Employees',
        'resourcetypes': 'Resource Types',
        'timesheets': 'Timesheets',
        'expensereports': 'Expense Reports',
        'accounts': 'Chart of Accounts',
        'journalentries': 'Journal Entries',
        'invoices': 'Invoices',
        'vendors': 'Vendors',
        'reports/income-statement': 'Income Statement',
        'reports/balance-sheet': 'Balance Sheet'
    };
    return titles[endpoint] || endpoint.charAt(0).toUpperCase() + endpoint.slice(1);
}

function renderDashboard(container, title, data) {
    let html = `
        <div class="mb-6">
            <h1 class="text-2xl font-bold text-gray-800">${title}</h1>
            <p class="text-gray-600">Overview and key metrics</p>
        </div>
    `;

    // Render dashboard cards
    html += '<div class="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-4 mb-6">';

    if (data.totalRevenue !== undefined) {
        html += createMetricCard('Total Revenue', formatCurrency(data.totalRevenue), 'bg-green-500');
    }
    if (data.totalExpenses !== undefined) {
        html += createMetricCard('Total Expenses', formatCurrency(data.totalExpenses), 'bg-red-500');
    }
    if (data.netIncome !== undefined) {
        html += createMetricCard('Net Income', formatCurrency(data.netIncome), 'bg-blue-500');
    }
    if (data.activeProjects !== undefined) {
        html += createMetricCard('Active Projects', data.activeProjects, 'bg-purple-500');
    }
    if (data.totalClients !== undefined) {
        html += createMetricCard('Total Clients', data.totalClients, 'bg-yellow-500');
    }
    if (data.totalEmployees !== undefined) {
        html += createMetricCard('Total Employees', data.totalEmployees, 'bg-indigo-500');
    }
    if (data.pendingInvoices !== undefined) {
        html += createMetricCard('Pending Invoices', data.pendingInvoices, 'bg-orange-500');
    }
    if (data.outstandingAR !== undefined) {
        html += createMetricCard('Outstanding A/R', formatCurrency(data.outstandingAR), 'bg-pink-500');
    }

    html += '</div>';

    // Render raw data as JSON for debugging
    html += `
        <div class="bg-white rounded-lg shadow p-4">
            <h3 class="font-semibold text-gray-700 mb-2">Raw Data</h3>
            <pre class="bg-gray-100 p-4 rounded overflow-auto text-sm">${JSON.stringify(data, null, 2)}</pre>
        </div>
    `;

    container.innerHTML = html;
}

function createMetricCard(label, value, bgColor) {
    return `
        <div class="bg-white rounded-lg shadow p-4">
            <div class="flex items-center">
                <div class="${bgColor} text-white p-3 rounded-lg">
                    <svg class="w-6 h-6" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                        <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M13 7h8m0 0v8m0-8l-8 8-4-4-6 6"/>
                    </svg>
                </div>
                <div class="ml-4">
                    <p class="text-sm text-gray-500">${label}</p>
                    <p class="text-xl font-bold text-gray-800">${value}</p>
                </div>
            </div>
        </div>
    `;
}

function renderList(container, title, endpoint, items, pagination = null) {
    let html = `
        <div class="mb-6 flex justify-between items-center">
            <div>
                <h1 class="text-2xl font-bold text-gray-800">${title}</h1>
                <p class="text-gray-600">${items.length} item(s) found</p>
            </div>
            <button onclick="showCreateModal('${endpoint}')" class="bg-primary text-white px-4 py-2 rounded-md hover:bg-secondary transition">
                + Add New
            </button>
        </div>
    `;

    if (items.length === 0) {
        html += `
            <div class="bg-white rounded-lg shadow p-8 text-center">
                <svg class="w-16 h-16 text-gray-400 mx-auto mb-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                    <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M20 13V6a2 2 0 00-2-2H6a2 2 0 00-2 2v7m16 0v5a2 2 0 01-2 2H6a2 2 0 01-2-2v-5m16 0h-2.586a1 1 0 00-.707.293l-2.414 2.414a1 1 0 01-.707.293h-3.172a1 1 0 01-.707-.293l-2.414-2.414A1 1 0 006.586 13H4"/>
                </svg>
                <p class="text-gray-500">No items found</p>
            </div>
        `;
    } else {
        html += '<div class="bg-white rounded-lg shadow overflow-hidden">';
        html += '<table class="min-w-full divide-y divide-gray-200">';

        // Table header
        const firstItem = items[0];
        const columns = Object.keys(firstItem).filter(key =>
            !key.toLowerCase().includes('password') &&
            !key.toLowerCase().includes('token') &&
            typeof firstItem[key] !== 'object'
        ).slice(0, 6);

        html += '<thead class="bg-gray-50"><tr>';
        columns.forEach(col => {
            html += `<th class="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">${formatColumnName(col)}</th>`;
        });
        html += '<th class="px-6 py-3 text-right text-xs font-medium text-gray-500 uppercase tracking-wider">Actions</th>';
        html += '</tr></thead>';

        // Table body
        html += '<tbody class="bg-white divide-y divide-gray-200">';
        items.forEach(item => {
            html += '<tr class="hover:bg-gray-50">';
            columns.forEach(col => {
                let value = item[col];
                if (value === null || value === undefined) {
                    value = '-';
                } else if (typeof value === 'boolean') {
                    value = value ? 'Yes' : 'No';
                } else if (col.toLowerCase().includes('date') && value) {
                    value = formatDate(value);
                } else if (col.toLowerCase().includes('amount') || col.toLowerCase().includes('balance') || col.toLowerCase().includes('salary')) {
                    value = formatCurrency(value);
                }
                html += `<td class="px-6 py-4 whitespace-nowrap text-sm text-gray-900">${value}</td>`;
            });
            html += `
                <td class="px-6 py-4 whitespace-nowrap text-right text-sm font-medium">
                    <button onclick="viewItem('${endpoint}', ${item.id})" class="text-primary hover:text-secondary mr-2">View</button>
                    <button onclick="editItem('${endpoint}', ${item.id})" class="text-yellow-600 hover:text-yellow-800 mr-2">Edit</button>
                </td>
            `;
            html += '</tr>';
        });
        html += '</tbody></table></div>';
    }

    container.innerHTML = html;
}

function renderReport(container, title, data) {
    let html = `
        <div class="mb-6">
            <h1 class="text-2xl font-bold text-gray-800">${title}</h1>
            <p class="text-gray-600">Financial report</p>
        </div>
        <div class="bg-white rounded-lg shadow p-6">
            <pre class="bg-gray-100 p-4 rounded overflow-auto text-sm">${JSON.stringify(data, null, 2)}</pre>
        </div>
    `;
    container.innerHTML = html;
}

function renderDetail(container, title, data) {
    let html = `
        <div class="mb-6">
            <h1 class="text-2xl font-bold text-gray-800">${title}</h1>
        </div>
        <div class="bg-white rounded-lg shadow p-6">
            <dl class="grid grid-cols-1 md:grid-cols-2 gap-4">
    `;

    Object.entries(data).forEach(([key, value]) => {
        if (typeof value !== 'object' && !key.toLowerCase().includes('password')) {
            html += `
                <div class="border-b border-gray-200 pb-2">
                    <dt class="text-sm font-medium text-gray-500">${formatColumnName(key)}</dt>
                    <dd class="mt-1 text-sm text-gray-900">${value ?? '-'}</dd>
                </div>
            `;
        }
    });

    html += '</dl></div>';
    container.innerHTML = html;
}

// Utility Functions
function formatCurrency(value) {
    if (value === null || value === undefined) return '-';
    return new Intl.NumberFormat('en-US', {
        style: 'currency',
        currency: 'USD'
    }).format(value);
}

function formatDate(dateString) {
    if (!dateString) return '-';
    return new Date(dateString).toLocaleDateString('en-US', {
        year: 'numeric',
        month: 'short',
        day: 'numeric'
    });
}

function formatColumnName(name) {
    return name
        .replace(/([A-Z])/g, ' $1')
        .replace(/^./, str => str.toUpperCase())
        .trim();
}

function showLoading() {
    document.getElementById('loading').classList.remove('hidden');
}

function hideLoading() {
    document.getElementById('loading').classList.add('hidden');
}

// Placeholder functions for CRUD operations
function showCreateModal(endpoint) {
    alert(`Create new ${endpoint} - Coming soon!`);
}

function viewItem(endpoint, id) {
    loadContent(`${endpoint}/${id}`);
}

function editItem(endpoint, id) {
    alert(`Edit ${endpoint} #${id} - Coming soon!`);
}
