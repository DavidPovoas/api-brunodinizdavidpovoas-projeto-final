function showMsg(elementId, text, type) {
  const el = document.getElementById(elementId);
  el.innerHTML = `<div class="msg ${type}">${text}</div>`;
  setTimeout(() => el.innerHTML = '', 4000);
}

function showApp() {
  document.getElementById('loginSection').style.display = 'none';
  document.getElementById('appSection').style.display = 'block';
  document.getElementById('btnLogout').style.display = 'block';
  loadProdutos();
}

function logout() {
  localStorage.removeItem('token');
  document.getElementById('loginSection').style.display = 'block';
  document.getElementById('appSection').style.display = 'none';
  document.getElementById('btnLogout').style.display = 'none';
}

async function login() {
  const email = document.getElementById('loginEmail').value;
  const pass = document.getElementById('loginPass').value;
  try {
    const res = await apiLogin(email, pass);
    if (!res.ok) { showMsg('loginMsg', 'Credenciais inválidas.', 'error'); return; }
    const data = await res.json();
    localStorage.setItem('token', data.token);
    showApp();
  } catch {
    showMsg('loginMsg', 'Erro ao ligar à API.', 'error');
  }
}

async function register() {
  const name = document.getElementById('regName').value;
  const email = document.getElementById('regEmail').value;
  const pass = document.getElementById('regPass').value;
  try {
    const res = await apiRegister(name, email, pass);
    if (!res.ok) { showMsg('loginMsg', 'Erro ao registar.', 'error'); return; }
    showMsg('loginMsg', 'Registado com sucesso! Faz login.', 'success');
    showTab('login');
  } catch {
    showMsg('loginMsg', 'Erro ao ligar à API.', 'error');
  }
}

function showTab(tab) {
  document.getElementById('tabLogin').style.display = tab === 'login' ? 'block' : 'none';
  document.getElementById('tabRegister').style.display = tab === 'register' ? 'block' : 'none';
  document.querySelectorAll('#loginSection .tabs .tab').forEach((t, i) => {
    t.classList.toggle('active', (tab === 'login' && i === 0) || (tab === 'register' && i === 1));
  });
}