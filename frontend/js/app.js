function showSection(sec) {
  document.querySelectorAll('.section').forEach(s => s.classList.remove('active'));
  document.getElementById('sec' + sec.charAt(0).toUpperCase() + sec.slice(1)).classList.add('active');
  document.querySelectorAll('#appSection > .tabs .tab').forEach((t, i) => {
    const secs = ['produtos', 'utilizadores', 'inventario', 'pagamentos'];
    t.classList.toggle('active', secs[i] === sec);
  });
  if (sec === 'produtos') loadProdutos();
  if (sec === 'utilizadores') loadUsers();
}

// PRODUTOS
async function loadProdutos() {
  try {
    const data = await apiGetProdutos();
    if (!data) return;
    const tbody = document.querySelector('#tblProdutos tbody');
    tbody.innerHTML = data.length === 0
      ? '<tr><td colspan="6" style="text-align:center;color:#999">Sem produtos</td></tr>'
      : data.map(p => `<tr>
          <td>${p.id}</td>
          <td>${p.name}</td>
          <td>${p.price}€</td>
          <td>${p.stock}</td>
          <td>${p.sku}</td>
          <td>
            <button class="btn btn-warning" onclick="editProduto(${p.id},'${p.name}','${p.description}',${p.price},${p.stock},'${p.sku}')">✏️</button>
            <button class="btn btn-danger" onclick="deleteProduto(${p.id})">🗑️</button>
          </td>
        </tr>`).join('');
  } catch {
    showMsg('prodMsg', 'Erro ao carregar produtos.', 'error');
  }
}

async function saveProduto() {
  const id = document.getElementById('pId').value;
  const body = {
    name: document.getElementById('pNome').value,
    description: document.getElementById('pDesc').value,
    price: parseFloat(document.getElementById('pPreco').value),
    stock: parseInt(document.getElementById('pStock').value),
    sku: document.getElementById('pSku').value,
    createdAt: new Date().toISOString()
  };
  try {
    if (id) {
      await apiUpdateProduto(id, { ...body, id: parseInt(id) });
      showMsg('prodMsg', 'Produto atualizado!', 'success');
    } else {
      await apiCreateProduto(body);
      showMsg('prodMsg', 'Produto criado!', 'success');
    }
    clearProdForm();
    loadProdutos();
  } catch {
    showMsg('prodMsg', 'Erro ao guardar produto.', 'error');
  }
}

function editProduto(id, name, desc, price, stock, sku) {
  document.getElementById('pId').value = id;
  document.getElementById('pNome').value = name;
  document.getElementById('pDesc').value = desc;
  document.getElementById('pPreco').value = price;
  document.getElementById('pStock').value = stock;
  document.getElementById('pSku').value = sku;
}

async function deleteProduto(id) {
  if (!confirm('Tens a certeza?')) return;
  try {
    await apiDeleteProduto(id);
    loadProdutos();
  } catch {
    showMsg('prodMsg', 'Erro ao eliminar.', 'error');
  }
}

function clearProdForm() {
  ['pId', 'pNome', 'pDesc', 'pPreco', 'pStock', 'pSku'].forEach(id => document.getElementById(id).value = '');
}

// UTILIZADORES
async function loadUsers() {
  try {
    const data = await apiGetUsers();
    if (!data) return;
    const tbody = document.querySelector('#tblUsers tbody');
    tbody.innerHTML = data.map(u => `<tr>
      <td>${u.id}</td>
      <td>${u.name}</td>
      <td>${u.email}</td>
      <td>${u.role}</td>
      <td>${new Date(u.createdAt).toLocaleDateString('pt-PT')}</td>
    </tr>`).join('');
  } catch {
    showMsg('userMsg', 'Erro ao carregar utilizadores.', 'error');
  }
}

// INVENTÁRIO
async function checkInventario() {
  const sku = document.getElementById('invSku').value;
  try {
    const data = await apiGetInventario(sku);
    if (!data) return;
    const el = document.getElementById('invResult');
    el.style.display = 'block';
    el.textContent = JSON.stringify(data, null, 2);
  } catch {
    showMsg('invMsg', 'Erro ao verificar inventário.', 'error');
  }
}

// PAGAMENTOS
async function processarPagamento() {
  const sku = document.getElementById('pagSku').value;
  const valor = parseFloat(document.getElementById('pagValor').value);
  try {
    const data = await apiPagamento({ sku, amount: valor });
    if (!data) return;
    const el = document.getElementById('pagResult');
    el.style.display = 'block';
    el.textContent = JSON.stringify(data, null, 2);
  } catch {
    showMsg('pagMsg', 'Erro ao processar pagamento.', 'error');
  }
}