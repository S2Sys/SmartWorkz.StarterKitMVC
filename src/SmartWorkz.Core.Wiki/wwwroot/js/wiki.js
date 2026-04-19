// SmartWorkz Core Wiki JavaScript

document.addEventListener('DOMContentLoaded', function () {
    // Expand active category in navigation
    const slugParam = new URLSearchParams(window.location.search).get('slug');
    if (slugParam) {
        const allLinks = document.querySelectorAll('.wiki-nav .list-group-item');
        allLinks.forEach(link => {
            const href = link.getAttribute('href');
            if (href && href.includes(slugParam)) {
                link.classList.add('active');

                // Expand parent accordion
                const accordion = link.closest('.accordion-collapse');
                if (accordion && !accordion.classList.contains('show')) {
                    const button = document.querySelector(`[data-bs-target="#${accordion.id}"]`);
                    if (button) {
                        button.click();
                    }
                }
            }
        });
    }

    // Smooth scroll to top when clicking brand
    document.querySelectorAll('.wiki-brand a').forEach(link => {
        link.addEventListener('click', function (e) {
            e.preventDefault();
            window.location.href = '/';
        });
    });

    // Copy code block functionality
    document.querySelectorAll('.wiki-article pre').forEach(pre => {
        const button = document.createElement('button');
        button.className = 'btn btn-sm btn-outline-secondary position-absolute top-0 end-0 m-2';
        button.innerHTML = '<i class="bi bi-clipboard"></i> Copy';
        button.style.opacity = '0';
        button.style.transition = 'opacity 0.2s';

        pre.style.position = 'relative';
        pre.appendChild(button);

        pre.addEventListener('mouseenter', () => button.style.opacity = '1');
        pre.addEventListener('mouseleave', () => button.style.opacity = '0');

        button.addEventListener('click', () => {
            const code = pre.textContent;
            navigator.clipboard.writeText(code).then(() => {
                button.innerHTML = '<i class="bi bi-check"></i> Copied!';
                setTimeout(() => {
                    button.innerHTML = '<i class="bi bi-clipboard"></i> Copy';
                }, 2000);
            });
        });
    });
});
