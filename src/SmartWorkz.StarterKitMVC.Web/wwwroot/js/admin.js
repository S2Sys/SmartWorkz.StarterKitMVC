/**
 * SmartWorkz StarterKitMVC - Admin JavaScript
 * Admin panel specific functionality
 */

'use strict';

// ============================================
// Admin Namespace
// ============================================
const Admin = window.Admin || {};

// ============================================
// Sidebar Management with Multi-Level Menu
// ============================================
Admin.sidebar = {
    sidebar: null,
    overlay: null,
    toggleBtn: null,
    closeBtn: null,
    
    init() {
        this.sidebar = document.getElementById('adminSidebar');
        this.overlay = document.getElementById('sidebarOverlay');
        this.toggleBtn = document.getElementById('sidebarToggle');
        this.closeBtn = document.getElementById('sidebarClose');
        
        if (!this.sidebar) return;
        
        // Toggle button click
        this.toggleBtn?.addEventListener('click', () => this.toggle());
        
        // Close button click (mobile)
        this.closeBtn?.addEventListener('click', () => this.close());
        
        // Overlay click (mobile)
        this.overlay?.addEventListener('click', () => this.close());
        
        // Close on escape key
        document.addEventListener('keydown', (e) => {
            if (e.key === 'Escape') this.close();
        });
        
        // Handle resize
        window.addEventListener('resize', this.debounce(() => {
            if (window.innerWidth >= 992) {
                this.sidebar.classList.remove('show');
                this.overlay?.classList.remove('show');
            }
        }, 100));
        
        // Restore collapsed state from storage
        const collapsed = localStorage.getItem('sidebar-collapsed');
        if (collapsed === 'true' && window.innerWidth >= 992) {
            this.sidebar.classList.add('collapsed');
        }
        
        // Initialize multi-level menus
        this.initMultiLevelMenu();
    },
    
    /**
     * Initialize multi-level dropdown menus in sidebar
     */
    initMultiLevelMenu() {
        const menuToggles = this.sidebar?.querySelectorAll('.nav-link[data-bs-toggle="collapse"]');
        
        menuToggles?.forEach(toggle => {
            toggle.addEventListener('click', (e) => {
                e.preventDefault();
                const targetId = toggle.getAttribute('data-bs-target') || toggle.getAttribute('href');
                const submenu = document.querySelector(targetId);
                
                if (submenu) {
                    // Close other open submenus at the same level
                    const parent = toggle.closest('.nav-item');
                    const siblings = parent?.parentElement?.querySelectorAll('.nav-item > .collapse.show');
                    siblings?.forEach(sibling => {
                        if (sibling !== submenu) {
                            sibling.classList.remove('show');
                            const siblingToggle = sibling.previousElementSibling;
                            siblingToggle?.classList.remove('active');
                            siblingToggle?.setAttribute('aria-expanded', 'false');
                        }
                    });
                    
                    // Toggle current submenu
                    submenu.classList.toggle('show');
                    toggle.classList.toggle('active');
                    toggle.setAttribute('aria-expanded', submenu.classList.contains('show'));
                }
            });
        });
        
        // Restore open state for active menu items
        const activeLinks = this.sidebar?.querySelectorAll('.nav-link.active');
        activeLinks?.forEach(link => {
            const parentCollapse = link.closest('.collapse');
            if (parentCollapse) {
                parentCollapse.classList.add('show');
                const parentToggle = parentCollapse.previousElementSibling;
                parentToggle?.classList.add('active');
                parentToggle?.setAttribute('aria-expanded', 'true');
            }
        });
    },
    
    toggle() {
        if (window.innerWidth < 992) {
            // Mobile: slide in/out
            this.sidebar.classList.toggle('show');
            this.overlay?.classList.toggle('show');
        } else {
            // Desktop: collapse/expand
            this.sidebar.classList.toggle('collapsed');
            localStorage.setItem('sidebar-collapsed', this.sidebar.classList.contains('collapsed'));
        }
    },
    
    open() {
        if (window.innerWidth < 992) {
            this.sidebar.classList.add('show');
            this.overlay?.classList.add('show');
        }
    },
    
    close() {
        if (window.innerWidth < 992) {
            this.sidebar.classList.remove('show');
            this.overlay?.classList.remove('show');
        }
    },
    
    debounce(func, wait) {
        let timeout;
        return function executedFunction(...args) {
            const later = () => {
                clearTimeout(timeout);
                func(...args);
            };
            clearTimeout(timeout);
            timeout = setTimeout(later, wait);
        };
    }
};

// ============================================
// Dashboard Widgets
// ============================================
Admin.dashboard = {
    /**
     * Refresh a dashboard widget
     * @param {string} widgetId - Widget element ID
     * @param {string} url - Data URL
     */
    async refreshWidget(widgetId, url) {
        const widget = document.getElementById(widgetId);
        if (!widget) return;
        
        const content = widget.querySelector('.widget-content');
        if (content) {
            content.innerHTML = '<div class="text-center py-4"><div class="spinner-border spinner-border-sm"></div></div>';
        }
        
        const result = await SW.http.get(url);
        if (result.success && content) {
            content.innerHTML = result.data.html || '';
        }
    },
    
    /**
     * Initialize auto-refresh for widgets
     * @param {number} interval - Refresh interval in ms
     */
    initAutoRefresh(interval = 60000) {
        const widgets = document.querySelectorAll('[data-auto-refresh]');
        widgets.forEach(widget => {
            const url = widget.getAttribute('data-auto-refresh');
            if (url) {
                setInterval(() => this.refreshWidget(widget.id, url), interval);
            }
        });
    }
};

// ============================================
// Data Tables Enhancement
// ============================================
Admin.dataTable = {
    /**
     * Initialize enhanced data table
     * @param {string} tableId - Table element ID
     * @param {Object} options - Configuration options
     */
    init(tableId, options = {}) {
        const table = document.getElementById(tableId);
        if (!table) return;
        
        const defaults = {
            searchable: true,
            sortable: true,
            paginate: true,
            pageSize: 10
        };
        
        const config = { ...defaults, ...options };
        
        // Add sortable class if needed
        if (config.sortable) {
            table.classList.add('sortable');
        }
        
        // Initialize search if exists
        const searchInput = document.querySelector(`[data-table-search="${tableId}"]`);
        if (searchInput && config.searchable) {
            searchInput.addEventListener('input', SW.utils.debounce((e) => {
                SW.table.filter(table, e.target.value);
            }, 300));
        }
    },
    
    /**
     * Select all rows in table
     * @param {string} tableId - Table element ID
     * @param {boolean} checked - Check state
     */
    selectAll(tableId, checked) {
        const table = document.getElementById(tableId);
        if (!table) return;
        
        const checkboxes = table.querySelectorAll('tbody input[type="checkbox"]');
        checkboxes.forEach(cb => cb.checked = checked);
        
        this.updateBulkActions(tableId);
    },
    
    /**
     * Get selected row IDs
     * @param {string} tableId - Table element ID
     * @returns {Array}
     */
    getSelectedIds(tableId) {
        const table = document.getElementById(tableId);
        if (!table) return [];
        
        const checked = table.querySelectorAll('tbody input[type="checkbox"]:checked');
        return Array.from(checked).map(cb => cb.value);
    },
    
    /**
     * Update bulk action buttons state
     * @param {string} tableId - Table element ID
     */
    updateBulkActions(tableId) {
        const selectedCount = this.getSelectedIds(tableId).length;
        const bulkActions = document.querySelector(`[data-bulk-actions="${tableId}"]`);
        
        if (bulkActions) {
            bulkActions.style.display = selectedCount > 0 ? 'block' : 'none';
            const countEl = bulkActions.querySelector('.selected-count');
            if (countEl) countEl.textContent = selectedCount;
        }
    }
};

// ============================================
// Form Helpers
// ============================================
Admin.form = {
    /**
     * Initialize form with AJAX submit
     * @param {string} formId - Form element ID
     * @param {Object} options - Configuration options
     */
    initAjax(formId, options = {}) {
        const form = document.getElementById(formId);
        if (!form) return;
        
        form.addEventListener('submit', async (e) => {
            e.preventDefault();
            
            if (!SW.form.validate(form)) return;
            
            const submitBtn = form.querySelector('[type="submit"]');
            const originalText = submitBtn?.innerHTML;
            
            if (submitBtn) {
                submitBtn.disabled = true;
                submitBtn.innerHTML = '<span class="spinner-border spinner-border-sm me-2"></span>Saving...';
            }
            
            const data = SW.form.serialize(form);
            const url = form.action || options.url;
            const method = form.method?.toUpperCase() || 'POST';
            
            const result = await SW.http.request(url, { method, body: data });
            
            if (submitBtn) {
                submitBtn.disabled = false;
                submitBtn.innerHTML = originalText;
            }
            
            if (result.success) {
                SW.toast.success(options.successMessage || 'Saved successfully!');
                if (options.onSuccess) options.onSuccess(result.data);
                if (options.redirectUrl) window.location.href = options.redirectUrl;
            } else {
                SW.toast.error(result.error?.message || 'An error occurred');
                if (options.onError) options.onError(result.error);
            }
        });
    },
    
    /**
     * Initialize dependent dropdowns
     * @param {string} parentId - Parent select ID
     * @param {string} childId - Child select ID
     * @param {string} url - Data URL (with {value} placeholder)
     */
    initDependentDropdown(parentId, childId, url) {
        const parent = document.getElementById(parentId);
        const child = document.getElementById(childId);
        
        if (!parent || !child) return;
        
        parent.addEventListener('change', async () => {
            const value = parent.value;
            child.innerHTML = '<option value="">Loading...</option>';
            child.disabled = true;
            
            if (!value) {
                child.innerHTML = '<option value="">Select...</option>';
                child.disabled = false;
                return;
            }
            
            const dataUrl = url.replace('{value}', value);
            const result = await SW.http.get(dataUrl);
            
            if (result.success) {
                child.innerHTML = '<option value="">Select...</option>';
                result.data.forEach(item => {
                    const option = document.createElement('option');
                    option.value = item.value;
                    option.textContent = item.text;
                    child.appendChild(option);
                });
            }
            
            child.disabled = false;
        });
    }
};

// ============================================
// Notifications
// ============================================
Admin.notifications = {
    /**
     * Mark notification as read
     * @param {string} notificationId - Notification ID
     */
    async markAsRead(notificationId) {
        const result = await SW.http.post(`/api/notifications/${notificationId}/read`);
        if (result.success) {
            const item = document.querySelector(`[data-notification-id="${notificationId}"]`);
            item?.classList.remove('unread');
            this.updateBadge();
        }
    },
    
    /**
     * Mark all notifications as read
     */
    async markAllAsRead() {
        const result = await SW.http.post('/api/notifications/read-all');
        if (result.success) {
            document.querySelectorAll('.notification-item.unread').forEach(item => {
                item.classList.remove('unread');
            });
            this.updateBadge();
        }
    },
    
    /**
     * Update notification badge count
     */
    updateBadge() {
        const unreadCount = document.querySelectorAll('.notification-item.unread').length;
        const badge = document.querySelector('.notification-badge');
        
        if (badge) {
            badge.textContent = unreadCount;
            badge.style.display = unreadCount > 0 ? 'inline-block' : 'none';
        }
    }
};

// ============================================
// Theme Management
// ============================================
Admin.theme = {
    init() {
        const themeToggle = document.getElementById('themeToggle');
        const themeIcon = document.getElementById('themeIcon');
        
        // Load saved theme
        const savedTheme = localStorage.getItem('admin-theme') || 'light';
        this.setTheme(savedTheme);
        
        themeToggle?.addEventListener('click', () => {
            const currentTheme = document.documentElement.getAttribute('data-bs-theme');
            const newTheme = currentTheme === 'dark' ? 'light' : 'dark';
            this.setTheme(newTheme);
            localStorage.setItem('admin-theme', newTheme);
        });
    },
    
    setTheme(theme) {
        document.documentElement.setAttribute('data-bs-theme', theme);
        const themeIcon = document.getElementById('themeIcon');
        if (themeIcon) {
            themeIcon.className = theme === 'dark' ? 'bi bi-sun' : 'bi bi-moon-stars';
        }
    }
};

// ============================================
// Smooth Scroll
// ============================================
Admin.smoothScroll = {
    init() {
        // Smooth scroll for anchor links
        document.querySelectorAll('a[href^="#"]').forEach(anchor => {
            anchor.addEventListener('click', (e) => {
                const targetId = anchor.getAttribute('href');
                if (targetId === '#' || targetId === '') return;
                
                const target = document.querySelector(targetId);
                if (target) {
                    e.preventDefault();
                    const offset = 80; // Account for fixed header
                    const targetPosition = target.getBoundingClientRect().top + window.pageYOffset - offset;
                    
                    window.scrollTo({
                        top: targetPosition,
                        behavior: 'smooth'
                    });
                }
            });
        });
    },
    
    /**
     * Scroll to element with offset
     * @param {string|HTMLElement} target - Target element or selector
     * @param {number} offset - Offset from top
     */
    to(target, offset = 80) {
        const el = typeof target === 'string' ? document.querySelector(target) : target;
        if (el) {
            const top = el.getBoundingClientRect().top + window.pageYOffset - offset;
            window.scrollTo({ top, behavior: 'smooth' });
        }
    }
};

// ============================================
// AOS (Animate On Scroll) - Lightweight Implementation
// ============================================
Admin.aos = {
    elements: [],
    
    init(options = {}) {
        const defaults = {
            offset: 100,
            duration: 600,
            easing: 'ease-out',
            once: true,
            mirror: false
        };
        
        this.options = { ...defaults, ...options };
        this.elements = document.querySelectorAll('[data-aos]');
        
        if (this.elements.length === 0) return;
        
        // Add initial styles
        this.elements.forEach(el => {
            el.style.opacity = '0';
            el.style.transition = `opacity ${this.options.duration}ms ${this.options.easing}, transform ${this.options.duration}ms ${this.options.easing}`;
            this.setInitialTransform(el);
        });
        
        // Check on scroll
        this.checkElements();
        window.addEventListener('scroll', this.throttle(() => this.checkElements(), 100));
        window.addEventListener('resize', this.throttle(() => this.checkElements(), 100));
    },
    
    setInitialTransform(el) {
        const animation = el.getAttribute('data-aos');
        const transforms = {
            'fade-up': 'translateY(30px)',
            'fade-down': 'translateY(-30px)',
            'fade-left': 'translateX(30px)',
            'fade-right': 'translateX(-30px)',
            'zoom-in': 'scale(0.9)',
            'zoom-out': 'scale(1.1)',
            'flip-up': 'rotateX(90deg)',
            'flip-down': 'rotateX(-90deg)',
            'slide-up': 'translateY(100%)',
            'slide-down': 'translateY(-100%)',
            'slide-left': 'translateX(100%)',
            'slide-right': 'translateX(-100%)'
        };
        
        if (transforms[animation]) {
            el.style.transform = transforms[animation];
        }
    },
    
    checkElements() {
        this.elements.forEach(el => {
            if (this.isInViewport(el)) {
                this.animate(el);
            } else if (this.options.mirror && !this.options.once) {
                this.reset(el);
            }
        });
    },
    
    isInViewport(el) {
        const rect = el.getBoundingClientRect();
        return (
            rect.top <= (window.innerHeight || document.documentElement.clientHeight) - this.options.offset &&
            rect.bottom >= 0
        );
    },
    
    animate(el) {
        if (el.classList.contains('aos-animate')) return;
        
        const delay = parseInt(el.getAttribute('data-aos-delay')) || 0;
        
        setTimeout(() => {
            el.style.opacity = '1';
            el.style.transform = 'translate(0) scale(1) rotate(0)';
            el.classList.add('aos-animate');
        }, delay);
    },
    
    reset(el) {
        el.style.opacity = '0';
        this.setInitialTransform(el);
        el.classList.remove('aos-animate');
    },
    
    throttle(func, limit) {
        let inThrottle;
        return function(...args) {
            if (!inThrottle) {
                func.apply(this, args);
                inThrottle = true;
                setTimeout(() => inThrottle = false, limit);
            }
        };
    }
};

// ============================================
// Charts Helper (Chart.js wrapper)
// ============================================
Admin.charts = {
    instances: {},
    
    /**
     * Create a line chart
     * @param {string} canvasId - Canvas element ID
     * @param {Object} config - Chart configuration
     */
    line(canvasId, config) {
        return this.create(canvasId, 'line', config);
    },
    
    /**
     * Create a bar chart
     * @param {string} canvasId - Canvas element ID
     * @param {Object} config - Chart configuration
     */
    bar(canvasId, config) {
        return this.create(canvasId, 'bar', config);
    },
    
    /**
     * Create a pie chart
     * @param {string} canvasId - Canvas element ID
     * @param {Object} config - Chart configuration
     */
    pie(canvasId, config) {
        return this.create(canvasId, 'pie', config);
    },
    
    /**
     * Create a doughnut chart
     * @param {string} canvasId - Canvas element ID
     * @param {Object} config - Chart configuration
     */
    doughnut(canvasId, config) {
        return this.create(canvasId, 'doughnut', config);
    },
    
    /**
     * Create an area chart
     * @param {string} canvasId - Canvas element ID
     * @param {Object} config - Chart configuration
     */
    area(canvasId, config) {
        config.fill = true;
        return this.create(canvasId, 'line', config);
    },
    
    /**
     * Create a radar chart
     * @param {string} canvasId - Canvas element ID
     * @param {Object} config - Chart configuration
     */
    radar(canvasId, config) {
        return this.create(canvasId, 'radar', config);
    },
    
    /**
     * Create a polar area chart
     * @param {string} canvasId - Canvas element ID
     * @param {Object} config - Chart configuration
     */
    polarArea(canvasId, config) {
        return this.create(canvasId, 'polarArea', config);
    },
    
    /**
     * Create a chart
     * @param {string} canvasId - Canvas element ID
     * @param {string} type - Chart type
     * @param {Object} config - Chart configuration
     */
    create(canvasId, type, config) {
        const canvas = document.getElementById(canvasId);
        if (!canvas || typeof Chart === 'undefined') {
            console.warn('Chart.js not loaded or canvas not found:', canvasId);
            return null;
        }
        
        // Destroy existing chart if any
        if (this.instances[canvasId]) {
            this.instances[canvasId].destroy();
        }
        
        const isDark = document.documentElement.getAttribute('data-bs-theme') === 'dark';
        const textColor = isDark ? '#adb5bd' : '#6c757d';
        const gridColor = isDark ? 'rgba(255,255,255,0.1)' : 'rgba(0,0,0,0.1)';
        
        const defaultOptions = {
            responsive: true,
            maintainAspectRatio: false,
            plugins: {
                legend: {
                    labels: { color: textColor }
                }
            },
            scales: type !== 'pie' && type !== 'doughnut' && type !== 'polarArea' && type !== 'radar' ? {
                x: {
                    ticks: { color: textColor },
                    grid: { color: gridColor }
                },
                y: {
                    ticks: { color: textColor },
                    grid: { color: gridColor }
                }
            } : undefined
        };
        
        const chartConfig = {
            type: type,
            data: config.data || {},
            options: { ...defaultOptions, ...config.options }
        };
        
        this.instances[canvasId] = new Chart(canvas, chartConfig);
        return this.instances[canvasId];
    },
    
    /**
     * Update chart data
     * @param {string} canvasId - Canvas element ID
     * @param {Object} data - New data
     */
    update(canvasId, data) {
        const chart = this.instances[canvasId];
        if (chart) {
            chart.data = data;
            chart.update();
        }
    },
    
    /**
     * Destroy a chart
     * @param {string} canvasId - Canvas element ID
     */
    destroy(canvasId) {
        if (this.instances[canvasId]) {
            this.instances[canvasId].destroy();
            delete this.instances[canvasId];
        }
    },
    
    /**
     * Get default color palette
     */
    getColors() {
        return {
            primary: '#0d6efd',
            success: '#198754',
            warning: '#ffc107',
            danger: '#dc3545',
            info: '#0dcaf0',
            secondary: '#6c757d',
            purple: '#6f42c1',
            pink: '#d63384',
            orange: '#fd7e14',
            teal: '#20c997'
        };
    },
    
    /**
     * Generate gradient for charts
     * @param {CanvasRenderingContext2D} ctx - Canvas context
     * @param {string} color - Base color
     */
    createGradient(ctx, color) {
        const gradient = ctx.createLinearGradient(0, 0, 0, 400);
        gradient.addColorStop(0, color);
        gradient.addColorStop(1, 'rgba(255,255,255,0)');
        return gradient;
    }
};

// ============================================
// Initialize on DOM Ready
// ============================================
document.addEventListener('DOMContentLoaded', function() {
    // Initialize sidebar
    Admin.sidebar.init();
    
    // Initialize theme
    Admin.theme.init();
    
    // Initialize smooth scroll
    Admin.smoothScroll.init();
    
    // Initialize AOS
    Admin.aos.init();
    
    // Initialize tooltips in admin
    document.querySelectorAll('[data-bs-toggle="tooltip"]').forEach(el => {
        new bootstrap.Tooltip(el);
    });
    
    // Table row checkbox handling - Fixed: Use direct event delegation
    document.addEventListener('change', function(e) {
        const checkbox = e.target;
        if (checkbox.matches('table tbody input[type="checkbox"]')) {
            const table = checkbox.closest('table');
            if (table?.id) {
                Admin.dataTable.updateBulkActions(table.id);
            }
        }
    });
    
    // Select all checkbox - Fixed: Use direct event delegation
    document.addEventListener('change', function(e) {
        const checkbox = e.target;
        if (checkbox.hasAttribute('data-select-all')) {
            const tableId = checkbox.getAttribute('data-select-all');
            Admin.dataTable.selectAll(tableId, checkbox.checked);
        }
    });
    
    // Confirm actions - Fixed: Use direct event delegation
    document.addEventListener('click', async function(e) {
        const target = e.target.closest('[data-confirm]');
        if (target) {
            e.preventDefault();
            const message = target.getAttribute('data-confirm');
            const confirmed = await SW.confirm.show({ message });
            if (confirmed) {
                const href = target.getAttribute('href');
                if (href) window.location.href = href;
            }
        }
    });
    
    console.log('Admin panel initialized');
});

// Export to window
window.Admin = Admin;
