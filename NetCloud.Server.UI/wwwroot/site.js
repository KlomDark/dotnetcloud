window.addEventListener('DOMContentLoaded', function () {
    // Ensure collapse works after initial load
    var navCollapse = document.getElementById('navbarNav');
    if (navCollapse) {
        new bootstrap.Collapse(navCollapse, { toggle: false });
    }
});

// Re-initialize collapse after Blazor navigation
window.addEventListener('locationchange', function () {
    var navCollapse = document.getElementById('navbarNav');
    if (navCollapse) {
        new bootstrap.Collapse(navCollapse, { toggle: false });
    }
});

// Blazor navigation event workaround
(function() {
    history.pushState = ((f) => function pushState(){
        var ret = f.apply(this, arguments);
        window.dispatchEvent(new Event('locationchange'));
        return ret;
    })(history.pushState);
    history.replaceState = ((f) => function replaceState(){
        var ret = f.apply(this, arguments);
        window.dispatchEvent(new Event('locationchange'));
        return ret;
    })(history.replaceState);
    window.addEventListener('popstate', function() {
        window.dispatchEvent(new Event('locationchange'));
    });
})();
