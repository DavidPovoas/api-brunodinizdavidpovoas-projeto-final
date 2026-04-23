const API_URL = 'http://localhost:5095/api';

function getToken() {
  return localStorage.getItem('token') || '';
}

function authHeaders() {
  return {
    'Content-Type': 'application/json',
    'Authorization': `Bearer ${getToken()}`
  };
}

async function apiFetch(endpoint, options = {}) {
  const res = await fetch(`${API_URL}${endpoint}`, options);
  if (res.status === 401) {
    logout();
    return null;
  }
  if (res.status === 204) return null;
  return res.json();
}

// AUTH
async function apiLogin(email, password) {
  return fetch(`${API_URL}/auth/login`, {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify({ email, password })
  });
}

async function apiRegister(name, email, password) {
  return fetch(`${API_URL}/auth/register`, {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify({ name, email, passwordHash: password, role: 'Customer' })
  });
}

// PRODUTOS
async function apiGetProdutos() {
  return apiFetch('/products', { headers: authHeaders() });
}
async function apiCreateProduto(data) {
  return apiFetch('/products', { method: 'POST', headers: authHeaders(), body: JSON.stringify(data) });
}
async function apiUpdateProduto(id, data) {
  return apiFetch(`/products/${id}`, { method: 'PUT', headers: authHeaders(), body: JSON.stringify(data) });
}
async function apiDeleteProduto(id) {
  return apiFetch(`/products/${id}`, { method: 'DELETE', headers: authHeaders() });
}

// UTILIZADORES
async function apiGetUsers() {
  return apiFetch('/users', { headers: authHeaders() });
}

// EXTERNOS
async function apiGetInventario(sku) {
  return apiFetch(`/external/inventory/${sku}`, { headers: authHeaders() });
}
async function apiPagamento(data) {
  return apiFetch('/external/payments', { method: 'POST', headers: authHeaders(), body: JSON.stringify(data) });
}