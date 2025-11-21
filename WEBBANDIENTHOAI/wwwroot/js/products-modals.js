// products-modals.js
(function(){
  async function fetchHtml(url){
    const res = await fetch(url, { headers: { 'X-Requested-With':'XMLHttpRequest' }, credentials:'same-origin' });
    if (!res.ok) throw new Error('Network error');
    return await res.text();
  }

  function attachModalCloseHandlers(root){
    root.querySelectorAll('.modal-close-btn, .btn-cancel').forEach(btn=>{
      btn.addEventListener('click', e=>{
        e.preventDefault();
        root.remove();
      });
    });
    // close when click background overlay
    root.addEventListener('click', e=>{
      if (e.target === root) root.remove();
    });
  }

  // open edit modal
  document.addEventListener('click', async function(e){
    const btn = e.target.closest('.btn-open-edit');
    if (!btn) return;
    e.preventDefault();
    const id = btn.dataset.id;
    try {
      window.showLoader?.();
      const html = await fetchHtml(`/Products/EditModal?id=${id}`);
      const container = document.createElement('div');
      container.innerHTML = html;
      document.body.appendChild(container);
      attachModalCloseHandlers(container);

      // attach submit
      const form = container.querySelector('#product-edit-form');
      form.addEventListener('submit', async function(ev){
        ev.preventDefault();
        const fd = new FormData(form);
        // include anti-forgery token if present
        const token = document.querySelector('input[name="__RequestVerificationToken"]');
        if (token) fd.append('__RequestVerificationToken', token.value);

        // show loader inside modal (optional)
        const res = await fetch('/Products/EditSubmit?id=' + id, {
          method: 'POST',
          body: fd,
          credentials: 'same-origin'
        });

        if (res.ok){
          const json = await res.json();
          if (json.success){
            // close modal and refresh list
            container.remove();
            await refreshList();
            alert(json.message || 'Lưu thành công');
          } else {
            alert('Lỗi: ' + (json.message || 'Không thành công'));
          }
        } else {
          const txt = await res.text();
          alert('Lỗi server: ' + res.status + '\n' + txt);
        }
      });

    } catch(err) {
      console.error(err);
      alert('Không thể tải form. Mở console để debug.');
    } finally {
      window.hideLoader?.();
    }
  });

  // open details modal (read-only)
  document.addEventListener('click', async function(e){
    const btn = e.target.closest('.btn-open-details');
    if (!btn) return;
    e.preventDefault();
    const id = btn.dataset.id;
    try {
      window.showLoader?.();
      const html = await fetchHtml(`/Products/DetailsModal?id=${id}`);
      const container = document.createElement('div');
      container.innerHTML = html;
      document.body.appendChild(container);
      attachModalCloseHandlers(container);
    } catch(err){
      console.error(err);
      alert('Không thể tải chi tiết.');
    } finally { window.hideLoader?.(); }
  });

  // refresh list (re-fetch current products panel)
  async function refreshList(){
    // find the current products URL from panel-root or location (you may store current filters)
    const panelRoot = document.getElementById('panel-root');
    const currentUrl = panelRoot?.dataset.currentUrl || window.location.pathname + window.location.search;
    try {
      const res = await fetch(currentUrl, { headers: { 'X-Requested-With':'XMLHttpRequest' }, credentials:'same-origin' });
      if (!res.ok) throw new Error('Fetch error');
      const html = await res.text();
      panelRoot.innerHTML = html;
    } catch(err){
      console.error(err);
    }
  }

  // filter form submit -> AJAX load
  document.addEventListener('submit', async function(e){
    if (e.target && e.target.id === 'product-filter-form'){
      e.preventDefault();
      const form = e.target;
      const params = new URLSearchParams(new FormData(form));
      const url = '/Products?' + params.toString();
      try {
        window.showLoader?.();
        const res = await fetch(url, { headers: { 'X-Requested-With':'XMLHttpRequest' }, credentials:'same-origin' });
        const html = await res.text();
        document.getElementById('panel-root').innerHTML = html;
      } catch(err){
        console.error(err);
      } finally { window.hideLoader?.(); }
    }
  });

})();
