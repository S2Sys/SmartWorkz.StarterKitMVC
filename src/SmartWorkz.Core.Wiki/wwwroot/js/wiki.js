// ============================================================================
// SmartWorkz.Core.Wiki - JavaScript Enhancement Suite
// Features: Active Nav, Mobile Toggle, Syntax Highlighting, TOC, Search
// ============================================================================

/**
 * 1. Initialize Active Navigation Highlighting
 * Marks current document as active in the sidebar accordion
 */
function initActiveNav() {
    const slug = new URLSearchParams(window.location.search).get('slug');
    if (!slug) return;

    const navLinks = document.querySelectorAll('.wiki-nav .list-group-item');
    navLinks.forEach(link => {
        const href = new URL(link.href, window.location.origin);
        const linkSlug = new URLSearchParams(href.search).get('slug');
        if (linkSlug === slug) {
            link.classList.add('active');
            // Expand parent accordion
            const parent = link.closest('.accordion-collapse');
            if (parent) {
                const button = parent.previousElementSibling.querySelector('.accordion-button');
                if (button && button.classList.contains('collapsed')) {
                    button.click();
                }
            }
        }
    });
}

/**
 * 2. Initialize Mobile Sidebar Toggle
 * Hamburger button toggles sidebar visibility on mobile
 */
function initSidebarToggle() {
    const btn = document.getElementById('wikiMenuToggle');
    const sidebar = document.querySelector('.wiki-sidebar');
    const overlay = document.getElementById('wikiOverlay');
    if (!btn || !sidebar) return;

    btn.addEventListener('click', () => {
        sidebar.classList.toggle('open');
        overlay.classList.toggle('show');
    });

    overlay.addEventListener('click', () => {
        sidebar.classList.remove('open');
        overlay.classList.remove('show');
    });

    // Close sidebar when clicking a link on mobile
    sidebar.querySelectorAll('a').forEach(link => {
        link.addEventListener('click', () => {
            if (window.innerWidth <= 768) {
                sidebar.classList.remove('open');
                overlay.classList.remove('show');
            }
        });
    });
}

/**
 * 3. Initialize Syntax Highlighting with Highlight.js
 * Wraps code blocks in styled containers with language labels and copy buttons
 */
function initSyntaxHighlighting() {
    if (typeof hljs === 'undefined') return;

    document.querySelectorAll('.wiki-article pre').forEach(pre => {
        const code = pre.querySelector('code');
        if (!code) return;

        // Extract language from class
        const langClass = [...code.classList].find(c => c.startsWith('language-'));
        const lang = langClass ? langClass.replace('language-', '') : '';
        const langLabel = lang || 'code';

        // Create wrapper structure
        const wrapper = document.createElement('div');
        wrapper.className = 'wiki-code-block';

        const header = document.createElement('div');
        header.className = 'wiki-code-header';

        const langSpan = document.createElement('span');
        langSpan.className = 'wiki-code-lang';
        langSpan.textContent = langLabel.toUpperCase();

        const copyBtn = document.createElement('button');
        copyBtn.className = 'wiki-copy-btn';
        copyBtn.innerHTML = '<i class="bi bi-clipboard"></i> Copy';
        copyBtn.type = 'button';

        header.appendChild(langSpan);
        header.appendChild(copyBtn);

        // Insert wrapper before pre
        pre.parentNode.insertBefore(wrapper, pre);
        wrapper.appendChild(header);
        wrapper.appendChild(pre);

        // Copy to clipboard handler
        copyBtn.addEventListener('click', () => {
            const text = code.textContent || '';
            navigator.clipboard.writeText(text).then(() => {
                copyBtn.innerHTML = '<i class="bi bi-check2"></i> Copied';
                copyBtn.classList.add('copied');
                setTimeout(() => {
                    copyBtn.innerHTML = '<i class="bi bi-clipboard"></i> Copy';
                    copyBtn.classList.remove('copied');
                }, 2000);
            });
        });

        // Apply syntax highlighting
        hljs.highlightElement(code);
    });
}

/**
 * 4. Initialize Table of Contents with Scroll Spy
 * Generates TOC from h2/h3 headings and highlights active section
 */
function initTableOfContents() {
    const article = document.querySelector('.wiki-article');
    const tocContainer = document.querySelector('.wiki-toc-inner');
    if (!article || !tocContainer) return;

    const headings = [...article.querySelectorAll('h2, h3')];
    if (headings.length < 2) {
        // Hide TOC if not enough headings
        const toc = document.querySelector('.wiki-toc');
        if (toc) toc.style.display = 'none';
        return;
    }

    // Assign IDs to headings without them
    headings.forEach((h, i) => {
        if (!h.id) {
            h.id = 'heading-' + h.textContent
                .toLowerCase()
                .replace(/[^a-z0-9\s-]/g, '')
                .replace(/\s+/g, '-')
                .replace(/-+/g, '-')
                .slice(0, 60) + '-' + i;
        }
    });

    // Build TOC structure
    const titleEl = document.createElement('div');
    titleEl.className = 'wiki-toc-title';
    titleEl.textContent = 'In this article';

    const list = document.createElement('ul');
    list.className = 'wiki-toc-list';

    headings.forEach(h => {
        const li = document.createElement('li');
        li.className = h.tagName === 'H3' ? 'toc-h3' : 'toc-h2';

        const a = document.createElement('a');
        a.href = '#' + h.id;
        a.textContent = h.textContent;

        a.addEventListener('click', e => {
            e.preventDefault();
            const target = document.getElementById(h.id);
            if (target) {
                target.scrollIntoView({ behavior: 'smooth', block: 'start' });
            }
        });

        li.appendChild(a);
        list.appendChild(li);
    });

    tocContainer.appendChild(titleEl);
    tocContainer.appendChild(list);

    // Intersection Observer for scroll spy
    const observer = new IntersectionObserver(entries => {
        entries.forEach(entry => {
            const id = entry.target.id;
            const tocLink = list.querySelector(`a[href="#${id}"]`);
            if (tocLink && entry.isIntersecting) {
                list.querySelectorAll('a').forEach(a => a.classList.remove('active'));
                tocLink.classList.add('active');
            }
        });
    }, { rootMargin: '0px 0px -70% 0px', threshold: 0 });

    headings.forEach(h => observer.observe(h));
}

/**
 * 5. Initialize Search Functionality
 * Client-side search with JSON index and keyboard navigation
 */
function initSearch() {
    const input = document.getElementById('wikiSearchInput');
    const results = document.getElementById('wikiSearchResults');
    if (!input || !results) return;

    let allDocs = null;
    let focusedIndex = -1;

    // Lazy-load docs index on first focus
    async function loadDocs() {
        if (allDocs !== null) return;
        try {
            const res = await fetch('/api/docs.json');
            allDocs = await res.json();
        } catch (e) {
            allDocs = [];
        }
    }

    function getResultItems() {
        return results.querySelectorAll('.wiki-search-result-item');
    }

    function setFocused(index) {
        const items = getResultItems();
        items.forEach(el => el.classList.remove('focused'));
        if (index >= 0 && index < items.length) {
            items[index].classList.add('focused');
            items[index].scrollIntoView({ block: 'nearest' });
        }
        focusedIndex = index;
    }

    function renderResults(query) {
        results.innerHTML = '';
        focusedIndex = -1;

        if (!query.trim()) {
            results.classList.remove('open');
            return;
        }

        const q = query.toLowerCase();
        const matched = allDocs
            .filter(d =>
                d.title.toLowerCase().includes(q) ||
                d.category.toLowerCase().includes(q)
            )
            .slice(0, 12);

        if (matched.length === 0) {
            results.innerHTML = '<div class="wiki-search-result-empty">No results found</div>';
        } else {
            matched.forEach(d => {
                const a = document.createElement('a');
                a.href = '/doc?slug=' + encodeURIComponent(d.slug);
                a.className = 'wiki-search-result-item';
                a.innerHTML = `${escapeHtml(d.title)}<span class="result-category">${escapeHtml(d.category)}</span>`;
                results.appendChild(a);
            });
        }
        results.classList.add('open');
    }

    function escapeHtml(str) {
        return str
            .replace(/&/g, '&amp;')
            .replace(/</g, '&lt;')
            .replace(/>/g, '&gt;');
    }

    input.addEventListener('focus', loadDocs);

    input.addEventListener('input', () => renderResults(input.value));

    input.addEventListener('keydown', e => {
        const items = getResultItems();
        if (e.key === 'ArrowDown') {
            e.preventDefault();
            setFocused(Math.min(focusedIndex + 1, items.length - 1));
        } else if (e.key === 'ArrowUp') {
            e.preventDefault();
            setFocused(Math.max(focusedIndex - 1, 0));
        } else if (e.key === 'Enter') {
            e.preventDefault();
            if (focusedIndex >= 0 && items[focusedIndex]) {
                window.location.href = items[focusedIndex].href;
            } else if (items.length > 0) {
                window.location.href = items[0].href;
            }
        } else if (e.key === 'Escape') {
            results.classList.remove('open');
            input.blur();
        }
    });

    // Close when clicking outside
    document.addEventListener('click', e => {
        if (!input.contains(e.target) && !results.contains(e.target)) {
            results.classList.remove('open');
        }
    });
}

/**
 * 6. Initialize Brand Link (Existing)
 * Soft redirect on brand click
 */
function initBrandLink() {
    const brand = document.querySelector('.wiki-brand a');
    if (brand) {
        brand.addEventListener('click', e => {
            e.preventDefault();
            window.location.href = '/';
        });
    }
}

// ============================================================================
// MAIN INITIALIZATION
// ============================================================================

document.addEventListener('DOMContentLoaded', function () {
    initActiveNav();
    initSidebarToggle();
    initSyntaxHighlighting();
    initTableOfContents();
    initSearch();
    initBrandLink();
});
