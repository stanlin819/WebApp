document.addEventListener('DOMContentLoaded', function() {
    initializeTransactionManager();
});

function initializeTransactionManager() {
    // 綁定 Modal 開啟事件
    document.addEventListener('click', function(e) {
        const btn = e.target.closest('.open-create-modal');
        if (!btn) return;
        e.preventDefault();
        openCreateModal(btn.getAttribute('data-url'));
    });

    document.addEventListener('click', function(e) {
        const deleteBtn = e.target.closest('.delete-transaction-btn');
        if (!deleteBtn) return;
        e.preventDefault();
        
        if (confirm('Are you sure you want to delete this transaction?')) {
            const transactionId = deleteBtn.getAttribute('data-id');
            const userId = deleteBtn.getAttribute('data-user-id');
            const page = deleteBtn.getAttribute('data-page');
            deleteTransaction(transactionId, userId, page);
        }
    });
}

function openCreateModal(url) {
    const modalEl = document.getElementById('createTransactionModal');
    const modalBody = modalEl.querySelector('.modal-body');
    modalBody.innerHTML = '<div class="text-center py-4">Loading...</div>';

    fetch(url, {
        headers: { 'X-Requested-With': 'XMLHttpRequest' }
    })
    .then(res => res.text())
    .then(html => {
        modalBody.innerHTML = html;
        const form = modalBody.querySelector('form');
        if (form) {
            form.addEventListener('submit', handleFormSubmit);
        }
        const modal = new bootstrap.Modal(modalEl);
        modal.show();
    })
    .catch(err => {
        console.error('Error loading modal:', err);
        modalBody.innerHTML = '<div class="alert alert-danger">Failed to load form</div>';
    });
}

function handleFormSubmit(e) {
    e.preventDefault();
    const form = e.target;
    const submitBtn = form.querySelector('button[type="submit"]');
    if (submitBtn) submitBtn.disabled = true;

    const formData = new FormData(form);

    fetch(form.action, {
        method: 'POST',
        body: formData,
        headers: {
            'X-Requested-With': 'XMLHttpRequest'
        }
    })
    .then(async res => {
        const contentType = res.headers.get("content-type");
        if (contentType && contentType.indexOf("application/json") !== -1) {
            const data = await res.json();
            if (data.success) {
                // 關閉 Modal
                const modalEl = document.getElementById('createTransactionModal');
                const modal = bootstrap.Modal.getInstance(modalEl);
                modal.hide();
                // 更新表格
                await refreshTransactionTable(data.userId);
                // 顯示成功訊息
                showAlert('success', data.message);
            }else {
                alert(data.message);
            }
        }else{
            const html = await res.text();
            const modalBody = document.getElementById('createTransactionModal').querySelector('.modal-body');
            modalBody.innerHTML = html;
            
            // 重新綁定新表單的提交事件
            const newForm = modalBody.querySelector('form');
            if (newForm) {
                newForm.addEventListener('submit', handleFormSubmit);
            }

        }
    })
    .catch(err => {
        showAlert('danger', err.message || 'Failed to create transaction');
    })
    .finally(() => {
        if (submitBtn) submitBtn.disabled = false;
    });
}

async function deleteTransaction(transactionId, userId, page) {
    try {
        const response = await fetch(`/Transaction/Delete?Id=${transactionId}&userId=${userId}&page=${page}`, {
            method: 'POST',
            headers: {
                'X-Requested-With': 'XMLHttpRequest'
            }
        });

        const data = await response.json();
        
        if (data.success) {
            await refreshTransactionTable(userId);
            showAlert('success', 'Transaction deleted successfully');
        } else {
            showAlert('danger', data.message || 'Failed to delete transaction');
        }
    } catch (err) {
        console.error('Delete error:', err);
        showAlert('danger', 'An error occurred while deleting the transaction');
    }
}

function refreshTransactionTable(userId) {
    const container = document.getElementById('transactionTableContainer');
    if (!container) return;

    const url = `/Transaction/TransactionTablePartial?userId=${userId}`;
    
    fetch(url, {
        headers: { 'X-Requested-With': 'XMLHttpRequest' }
    })
    .then(res => res.text())
    .then(html => {
        container.innerHTML = html;
    })
    .catch(err => {
        console.error('Error refreshing table:', err);
        window.location.reload();
    });
}

function showAlert(type, message) {
    const alertDiv = document.createElement('div');
    alertDiv.className = `alert alert-${type} alert-dismissible fade show`;
    alertDiv.innerHTML = `
        ${message}
        <button type="button" class="btn-close" data-bs-dismiss="alert" aria-label="Close"></button>
    `;
    
    const container = document.querySelector('.container');
    container.insertBefore(alertDiv, container.firstChild);
    
    setTimeout(() => alertDiv.remove(), 3000);
}
