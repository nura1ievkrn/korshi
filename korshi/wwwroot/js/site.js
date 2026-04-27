// Körши — site.js

document.addEventListener('DOMContentLoaded', function () {

    // ── Sidebar toggle (mobile) ──────────────────────────
    const sidebar = document.getElementById('sidebar');
    const sidebarToggle = document.getElementById('sidebarToggle');
    const sidebarOverlay = document.getElementById('sidebarOverlay');

    function openSidebar() {
        sidebar?.classList.add('kw-sidebar--open');
        sidebarOverlay?.classList.add('kw-sidebar-overlay--visible');
        document.body.style.overflow = 'hidden';
    }

    function closeSidebar() {
        sidebar?.classList.remove('kw-sidebar--open');
        sidebarOverlay?.classList.remove('kw-sidebar-overlay--visible');
        document.body.style.overflow = '';
    }

    sidebarToggle?.addEventListener('click', function () {
        sidebar?.classList.contains('kw-sidebar--open') ? closeSidebar() : openSidebar();
    });

    sidebarOverlay?.addEventListener('click', closeSidebar);

    // ── Filter tabs ──────────────────────────────────────
    document.querySelectorAll('.kw-tab').forEach(function (tab) {
        tab.addEventListener('click', function () {
            const group = tab.closest('.kw-tabs');
            group?.querySelectorAll('.kw-tab').forEach(t => t.classList.remove('kw-tab--active'));
            tab.classList.add('kw-tab--active');

            // Filter cards if data-category attribute present
            const cat = tab.dataset.category;
            if (cat) {
                document.querySelectorAll('.kw-card').forEach(function (card) {
                    if (cat === 'all' || card.dataset.category === cat) {
                        card.style.display = '';
                    } else {
                        card.style.display = 'none';
                    }
                });
            }
        });
    });

    // ── Toast notifications ──────────────────────────────
    window.kwToast = function (message, type) {
        const container = document.getElementById('toastContainer');
        if (!container) return;
        const toast = document.createElement('div');
        toast.className = 'kw-toast';
        if (type === 'success') toast.style.background = '#065F46';
        if (type === 'error') toast.style.background = '#7F1D1D';
        toast.textContent = message;
        container.appendChild(toast);
        setTimeout(function () {
            toast.style.transition = 'opacity 0.3s';
            toast.style.opacity = '0';
            setTimeout(function () { toast.remove(); }, 350);
        }, 3500);
    };

    // ── Count-up animation for stat numbers ─────────────
    document.querySelectorAll('[data-countup]').forEach(function (el) {
        const target = parseInt(el.dataset.countup, 10);
        const duration = 1000;
        const start = performance.now();
        function step(now) {
            const elapsed = Math.min((now - start) / duration, 1);
            el.textContent = Math.floor(elapsed * target);
            if (elapsed < 1) requestAnimationFrame(step);
            else el.textContent = target;
        }
        requestAnimationFrame(step);
    });

    // ── Animate progress bars on load ───────────────────
    setTimeout(function () {
        document.querySelectorAll('.kw-progress__fill').forEach(function (bar) {
            const pct = bar.dataset.pct || 0;
            bar.style.width = pct + '%';
        });
    }, 200);

});