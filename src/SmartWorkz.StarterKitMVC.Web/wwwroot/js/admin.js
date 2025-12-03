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
// Sidebar Management
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
// Initialize on DOM Ready
// ============================================
document.addEventListener('DOMContentLoaded', function() {
    // Initialize sidebar
    Admin.sidebar.init();
    
    // Initialize theme
    Admin.theme.init();
    
    // Initialize tooltips in admin
    document.querySelectorAll('[data-bs-toggle="tooltip"]').forEach(el => {
        new bootstrap.Tooltip(el);
    });
    
    // Table row checkbox handling
    SW.on('change', 'table tbody input[type="checkbox"]', function() {
        const table = this.closest('table');
        if (table?.id) {
            Admin.dataTable.updateBulkActions(table.id);
        }
    });
    
    // Select all checkbox
    SW.on('change', '[data-select-all]', function() {
        const tableId = this.getAttribute('data-select-all');
        Admin.dataTable.selectAll(tableId, this.checked);
    });
    
    // Confirm actions
    SW.on('click', '[data-confirm]', async function(e) {
        e.preventDefault();
        const message = this.getAttribute('data-confirm');
        const confirmed = await SW.confirm.show({ message });
        if (confirmed) {
            const href = this.getAttribute('href');
            if (href) window.location.href = href;
        }
    });
    
    console.log('Admin panel initialized');
});

// Export to window
window.Admin = Admin;
