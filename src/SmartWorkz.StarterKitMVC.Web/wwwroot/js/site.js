/**
 * SmartWorkz StarterKitMVC - Site JavaScript
 * Comprehensive utility library for Bootstrap 5.3.3
 * 
 * @version 1.0.0
 * @author SmartWorkz
 */

'use strict';

// ============================================
// Global Namespace
// ============================================
const SW = window.SW || {};

// ============================================
// Configuration
// ============================================
SW.config = {
    apiBaseUrl: '/api',
    toastDuration: 5000,
    debounceDelay: 300,
    animationDuration: 300
};

// ============================================
// Utility Functions
// ============================================
SW.utils = {
    /**
     * Debounce function execution
     * @param {Function} func - Function to debounce
     * @param {number} wait - Wait time in ms
     * @returns {Function}
     * @example SW.utils.debounce(() => console.log('Debounced'), 300)
     */
    debounce(func, wait = SW.config.debounceDelay) {
        let timeout;
        return function executedFunction(...args) {
            const later = () => {
                clearTimeout(timeout);
                func(...args);
            };
            clearTimeout(timeout);
            timeout = setTimeout(later, wait);
        };
    },

    /**
     * Throttle function execution
     * @param {Function} func - Function to throttle
     * @param {number} limit - Limit in ms
     * @returns {Function}
     */
    throttle(func, limit = 100) {
        let inThrottle;
        return function(...args) {
            if (!inThrottle) {
                func.apply(this, args);
                inThrottle = true;
                setTimeout(() => inThrottle = false, limit);
            }
        };
    },

    /**
     * Deep clone an object
     * @param {Object} obj - Object to clone
     * @returns {Object}
     */
    deepClone(obj) {
        return JSON.parse(JSON.stringify(obj));
    },

    /**
     * Generate unique ID
     * @param {string} prefix - Optional prefix
     * @returns {string}
     */
    generateId(prefix = 'sw') {
        return `${prefix}_${Date.now()}_${Math.random().toString(36).substr(2, 9)}`;
    },

    /**
     * Format date to locale string
     * @param {Date|string} date - Date to format
     * @param {Object} options - Intl.DateTimeFormat options
     * @returns {string}
     */
    formatDate(date, options = {}) {
        const d = new Date(date);
        return d.toLocaleDateString(undefined, {
            year: 'numeric',
            month: 'short',
            day: 'numeric',
            ...options
        });
    },

    /**
     * Format number with separators
     * @param {number} num - Number to format
     * @param {number} decimals - Decimal places
     * @returns {string}
     */
    formatNumber(num, decimals = 0) {
        return new Intl.NumberFormat(undefined, {
            minimumFractionDigits: decimals,
            maximumFractionDigits: decimals
        }).format(num);
    },

    /**
     * Format currency
     * @param {number} amount - Amount to format
     * @param {string} currency - Currency code
     * @returns {string}
     */
    formatCurrency(amount, currency = 'USD') {
        return new Intl.NumberFormat(undefined, {
            style: 'currency',
            currency: currency
        }).format(amount);
    },

    /**
     * Truncate text with ellipsis
     * @param {string} text - Text to truncate
     * @param {number} length - Max length
     * @returns {string}
     */
    truncate(text, length = 100) {
        if (text.length <= length) return text;
        return text.substring(0, length).trim() + '...';
    },

    /**
     * Escape HTML entities
     * @param {string} str - String to escape
     * @returns {string}
     */
    escapeHtml(str) {
        const div = document.createElement('div');
        div.textContent = str;
        return div.innerHTML;
    },

    /**
     * Parse query string to object
     * @param {string} queryString - Query string
     * @returns {Object}
     */
    parseQueryString(queryString = window.location.search) {
        return Object.fromEntries(new URLSearchParams(queryString));
    },

    /**
     * Build query string from object
     * @param {Object} params - Parameters object
     * @returns {string}
     */
    buildQueryString(params) {
        return new URLSearchParams(params).toString();
    },

    /**
     * Check if element is in viewport
     * @param {HTMLElement} el - Element to check
     * @returns {boolean}
     */
    isInViewport(el) {
        const rect = el.getBoundingClientRect();
        return (
            rect.top >= 0 &&
            rect.left >= 0 &&
            rect.bottom <= (window.innerHeight || document.documentElement.clientHeight) &&
            rect.right <= (window.innerWidth || document.documentElement.clientWidth)
        );
    },

    /**
     * Smooth scroll to element
     * @param {string|HTMLElement} target - Target element or selector
     * @param {number} offset - Offset from top
     */
    scrollTo(target, offset = 0) {
        const el = typeof target === 'string' ? document.querySelector(target) : target;
        if (el) {
            const top = el.getBoundingClientRect().top + window.pageYOffset - offset;
            window.scrollTo({ top, behavior: 'smooth' });
        }
    },

    /**
     * Copy text to clipboard
     * @param {string} text - Text to copy
     * @returns {Promise<boolean>}
     */
    async copyToClipboard(text) {
        try {
            await navigator.clipboard.writeText(text);
            return true;
        } catch (err) {
            console.error('Failed to copy:', err);
            return false;
        }
    }
};

// ============================================
// Storage Helpers
// ============================================
SW.storage = {
    /**
     * Set item in localStorage with optional expiry
     * @param {string} key - Storage key
     * @param {*} value - Value to store
     * @param {number} expiryMs - Expiry time in ms (optional)
     */
    set(key, value, expiryMs = null) {
        const item = {
            value: value,
            expiry: expiryMs ? Date.now() + expiryMs : null
        };
        localStorage.setItem(key, JSON.stringify(item));
    },

    /**
     * Get item from localStorage
     * @param {string} key - Storage key
     * @param {*} defaultValue - Default value if not found
     * @returns {*}
     */
    get(key, defaultValue = null) {
        const itemStr = localStorage.getItem(key);
        if (!itemStr) return defaultValue;
        
        try {
            const item = JSON.parse(itemStr);
            if (item.expiry && Date.now() > item.expiry) {
                localStorage.removeItem(key);
                return defaultValue;
            }
            return item.value;
        } catch {
            return defaultValue;
        }
    },

    /**
     * Remove item from localStorage
     * @param {string} key - Storage key
     */
    remove(key) {
        localStorage.removeItem(key);
    },

    /**
     * Clear all localStorage
     */
    clear() {
        localStorage.clear();
    }
};

// ============================================
// Toast Notifications
// ============================================
SW.toast = {
    /**
     * Show toast notification
     * @param {string} message - Message to display
     * @param {string} type - Type: success, danger, warning, info
     * @param {number} duration - Duration in ms
     */
    show(message, type = 'info', duration = SW.config.toastDuration) {
        const container = document.getElementById('toastContainer');
        const template = document.getElementById('toastTemplate');
        
        if (!container || !template) {
            console.warn('Toast container or template not found');
            return;
        }

        const icons = {
            success: 'bi-check-circle-fill',
            danger: 'bi-exclamation-triangle-fill',
            warning: 'bi-exclamation-circle-fill',
            info: 'bi-info-circle-fill'
        };

        const clone = template.content.cloneNode(true);
        const toast = clone.querySelector('.toast');
        const icon = clone.querySelector('.toast-icon');
        const msg = clone.querySelector('.toast-message');

        toast.classList.add(`bg-${type}`, 'text-white');
        icon.classList.add(icons[type] || icons.info);
        msg.textContent = message;

        container.appendChild(clone);
        
        const bsToast = new bootstrap.Toast(toast, { delay: duration });
        bsToast.show();

        toast.addEventListener('hidden.bs.toast', () => toast.remove());
    },

    success(message, duration) { this.show(message, 'success', duration); },
    error(message, duration) { this.show(message, 'danger', duration); },
    warning(message, duration) { this.show(message, 'warning', duration); },
    info(message, duration) { this.show(message, 'info', duration); }
};

// ============================================
// Loading Overlay
// ============================================
SW.loading = {
    /**
     * Show loading overlay
     * @param {string} message - Loading message
     */
    show(message = 'Loading...') {
        const overlay = document.getElementById('loadingOverlay');
        const msgEl = document.getElementById('loadingMessage');
        
        if (overlay) {
            if (msgEl) msgEl.textContent = message;
            overlay.classList.remove('d-none');
        }
    },

    /**
     * Hide loading overlay
     */
    hide() {
        const overlay = document.getElementById('loadingOverlay');
        if (overlay) {
            overlay.classList.add('d-none');
        }
    }
};

// ============================================
// Confirm Dialog
// ============================================
SW.confirm = {
    /**
     * Show confirmation dialog
     * @param {Object} options - Dialog options
     * @returns {Promise<boolean>}
     */
    show(options = {}) {
        return new Promise((resolve) => {
            const modal = document.getElementById('confirmModal');
            const title = document.getElementById('confirmModalTitle');
            const body = document.getElementById('confirmModalBody');
            const okBtn = document.getElementById('confirmModalOk');

            if (!modal) {
                resolve(window.confirm(options.message || 'Are you sure?'));
                return;
            }

            if (title) title.textContent = options.title || 'Confirm Action';
            if (body) body.textContent = options.message || 'Are you sure you want to proceed?';
            if (okBtn) {
                okBtn.textContent = options.okText || 'Confirm';
                okBtn.className = `btn btn-${options.okClass || 'primary'}`;
            }

            const bsModal = new bootstrap.Modal(modal);
            
            const handleOk = () => {
                bsModal.hide();
                resolve(true);
            };

            const handleCancel = () => {
                resolve(false);
            };

            okBtn?.addEventListener('click', handleOk, { once: true });
            modal.addEventListener('hidden.bs.modal', handleCancel, { once: true });

            bsModal.show();
        });
    },

    /**
     * Show delete confirmation
     * @param {string} itemName - Name of item to delete
     * @returns {Promise<boolean>}
     */
    delete(itemName = 'this item') {
        return this.show({
            title: 'Confirm Delete',
            message: `Are you sure you want to delete ${itemName}? This action cannot be undone.`,
            okText: 'Delete',
            okClass: 'danger'
        });
    }
};

// ============================================
// AJAX / Fetch Helpers
// ============================================
SW.http = {
    /**
     * Make HTTP request
     * @param {string} url - Request URL
     * @param {Object} options - Fetch options
     * @returns {Promise<Object>}
     */
    async request(url, options = {}) {
        const defaults = {
            headers: {
                'Content-Type': 'application/json',
                'X-Requested-With': 'XMLHttpRequest'
            }
        };

        const config = { ...defaults, ...options };
        
        if (config.body && typeof config.body === 'object') {
            config.body = JSON.stringify(config.body);
        }

        try {
            const response = await fetch(url, config);
            const data = await response.json().catch(() => ({}));
            
            if (!response.ok) {
                throw { status: response.status, data };
            }
            
            return { success: true, data, status: response.status };
        } catch (error) {
            return { 
                success: false, 
                error: error.data || error.message || 'Request failed',
                status: error.status || 0
            };
        }
    },

    get(url, params = {}) {
        const queryString = SW.utils.buildQueryString(params);
        const fullUrl = queryString ? `${url}?${queryString}` : url;
        return this.request(fullUrl, { method: 'GET' });
    },

    post(url, body = {}) {
        return this.request(url, { method: 'POST', body });
    },

    put(url, body = {}) {
        return this.request(url, { method: 'PUT', body });
    },

    delete(url) {
        return this.request(url, { method: 'DELETE' });
    }
};

// ============================================
// Form Helpers
// ============================================
SW.form = {
    /**
     * Serialize form to object
     * @param {HTMLFormElement|string} form - Form element or selector
     * @returns {Object}
     */
    serialize(form) {
        const formEl = typeof form === 'string' ? document.querySelector(form) : form;
        if (!formEl) return {};
        
        const formData = new FormData(formEl);
        const obj = {};
        
        formData.forEach((value, key) => {
            if (obj[key]) {
                if (!Array.isArray(obj[key])) obj[key] = [obj[key]];
                obj[key].push(value);
            } else {
                obj[key] = value;
            }
        });
        
        return obj;
    },

    /**
     * Populate form with data
     * @param {HTMLFormElement|string} form - Form element or selector
     * @param {Object} data - Data to populate
     */
    populate(form, data) {
        const formEl = typeof form === 'string' ? document.querySelector(form) : form;
        if (!formEl || !data) return;

        Object.keys(data).forEach(key => {
            const field = formEl.elements[key];
            if (!field) return;

            if (field.type === 'checkbox') {
                field.checked = Boolean(data[key]);
            } else if (field.type === 'radio') {
                const radio = formEl.querySelector(`[name="${key}"][value="${data[key]}"]`);
                if (radio) radio.checked = true;
            } else {
                field.value = data[key];
            }
        });
    },

    /**
     * Reset form and clear validation
     * @param {HTMLFormElement|string} form - Form element or selector
     */
    reset(form) {
        const formEl = typeof form === 'string' ? document.querySelector(form) : form;
        if (!formEl) return;
        
        formEl.reset();
        formEl.classList.remove('was-validated');
        formEl.querySelectorAll('.is-invalid, .is-valid').forEach(el => {
            el.classList.remove('is-invalid', 'is-valid');
        });
    },

    /**
     * Validate form using Bootstrap validation
     * @param {HTMLFormElement|string} form - Form element or selector
     * @returns {boolean}
     */
    validate(form) {
        const formEl = typeof form === 'string' ? document.querySelector(form) : form;
        if (!formEl) return false;
        
        formEl.classList.add('was-validated');
        return formEl.checkValidity();
    },

    /**
     * Show field error
     * @param {HTMLElement|string} field - Field element or selector
     * @param {string} message - Error message
     */
    showError(field, message) {
        const fieldEl = typeof field === 'string' ? document.querySelector(field) : field;
        if (!fieldEl) return;

        fieldEl.classList.add('is-invalid');
        
        let feedback = fieldEl.nextElementSibling;
        if (!feedback || !feedback.classList.contains('invalid-feedback')) {
            feedback = document.createElement('div');
            feedback.className = 'invalid-feedback';
            fieldEl.parentNode.insertBefore(feedback, fieldEl.nextSibling);
        }
        feedback.textContent = message;
    },

    /**
     * Clear field error
     * @param {HTMLElement|string} field - Field element or selector
     */
    clearError(field) {
        const fieldEl = typeof field === 'string' ? document.querySelector(field) : field;
        if (!fieldEl) return;
        
        fieldEl.classList.remove('is-invalid');
    }
};

// ============================================
// Table Helpers
// ============================================
SW.table = {
    /**
     * Sort table by column
     * @param {HTMLTableElement|string} table - Table element or selector
     * @param {number} columnIndex - Column index to sort
     * @param {boolean} ascending - Sort direction
     */
    sort(table, columnIndex, ascending = true) {
        const tableEl = typeof table === 'string' ? document.querySelector(table) : table;
        if (!tableEl) return;

        const tbody = tableEl.querySelector('tbody');
        const rows = Array.from(tbody.querySelectorAll('tr'));

        rows.sort((a, b) => {
            const aVal = a.cells[columnIndex]?.textContent.trim() || '';
            const bVal = b.cells[columnIndex]?.textContent.trim() || '';
            
            const aNum = parseFloat(aVal);
            const bNum = parseFloat(bVal);
            
            if (!isNaN(aNum) && !isNaN(bNum)) {
                return ascending ? aNum - bNum : bNum - aNum;
            }
            
            return ascending ? aVal.localeCompare(bVal) : bVal.localeCompare(aVal);
        });

        rows.forEach(row => tbody.appendChild(row));
    },

    /**
     * Filter table rows
     * @param {HTMLTableElement|string} table - Table element or selector
     * @param {string} query - Search query
     */
    filter(table, query) {
        const tableEl = typeof table === 'string' ? document.querySelector(table) : table;
        if (!tableEl) return;

        const rows = tableEl.querySelectorAll('tbody tr');
        const searchTerm = query.toLowerCase();

        rows.forEach(row => {
            const text = row.textContent.toLowerCase();
            row.style.display = text.includes(searchTerm) ? '' : 'none';
        });
    },

    /**
     * Export table to CSV
     * @param {HTMLTableElement|string} table - Table element or selector
     * @param {string} filename - Output filename
     */
    exportCsv(table, filename = 'export.csv') {
        const tableEl = typeof table === 'string' ? document.querySelector(table) : table;
        if (!tableEl) return;

        const rows = tableEl.querySelectorAll('tr');
        const csv = [];

        rows.forEach(row => {
            const cols = row.querySelectorAll('td, th');
            const rowData = Array.from(cols).map(col => {
                let text = col.textContent.trim();
                text = text.replace(/"/g, '""');
                return `"${text}"`;
            });
            csv.push(rowData.join(','));
        });

        const blob = new Blob([csv.join('\n')], { type: 'text/csv;charset=utf-8;' });
        const link = document.createElement('a');
        link.href = URL.createObjectURL(blob);
        link.download = filename;
        link.click();
    }
};

// ============================================
// Theme Toggle
// ============================================
SW.theme = {
    /**
     * Get current theme
     * @returns {string}
     */
    get() {
        return document.documentElement.getAttribute('data-bs-theme') || 'light';
    },

    /**
     * Set theme
     * @param {string} theme - Theme name: light or dark
     */
    set(theme) {
        document.documentElement.setAttribute('data-bs-theme', theme);
        SW.storage.set('theme', theme);
        this.updateIcon();
    },

    /**
     * Toggle between light and dark
     */
    toggle() {
        const current = this.get();
        this.set(current === 'light' ? 'dark' : 'light');
    },

    /**
     * Update theme icon
     */
    updateIcon() {
        const icon = document.getElementById('themeIcon');
        if (icon) {
            const isDark = this.get() === 'dark';
            icon.className = isDark ? 'bi bi-sun-fill' : 'bi bi-moon-stars';
        }
    },

    /**
     * Initialize theme from storage
     */
    init() {
        const saved = SW.storage.get('theme');
        if (saved) {
            this.set(saved);
        } else if (window.matchMedia('(prefers-color-scheme: dark)').matches) {
            this.set('dark');
        }
        this.updateIcon();
    }
};

// ============================================
// Event Delegation
// ============================================
SW.on = function(eventType, selector, handler) {
    document.addEventListener(eventType, function(e) {
        const target = e.target.closest(selector);
        if (target) {
            handler.call(target, e, target);
        }
    });
};

// ============================================
// Initialize on DOM Ready
// ============================================
document.addEventListener('DOMContentLoaded', function() {
    // Initialize theme
    SW.theme.init();

    // Theme toggle button
    SW.on('click', '#themeToggle', () => SW.theme.toggle());

    // Table search
    SW.on('input', '[data-table-search]', SW.utils.debounce(function(e) {
        const tableId = this.getAttribute('data-table-search');
        SW.table.filter(`#${tableId}`, this.value);
    }));

    // Table sort
    SW.on('click', '.table.sortable th', function() {
        const table = this.closest('table');
        const index = Array.from(this.parentNode.children).indexOf(this);
        const isAsc = this.classList.contains('sort-asc');
        
        table.querySelectorAll('th').forEach(th => th.classList.remove('sort-asc', 'sort-desc'));
        this.classList.add(isAsc ? 'sort-desc' : 'sort-asc');
        
        SW.table.sort(table, index, !isAsc);
    });

    // Table export CSV
    SW.on('click', '[data-table-export="csv"]', function() {
        const tableId = this.getAttribute('data-table');
        SW.table.exportCsv(`#${tableId}`);
    });

    // Copy to clipboard
    SW.on('click', '[data-copy]', async function() {
        const text = this.getAttribute('data-copy') || this.textContent;
        const success = await SW.utils.copyToClipboard(text);
        if (success) {
            SW.toast.success('Copied to clipboard!');
        }
    });

    // Confirm delete
    SW.on('click', '[data-confirm-delete]', async function(e) {
        e.preventDefault();
        const itemName = this.getAttribute('data-confirm-delete') || 'this item';
        const confirmed = await SW.confirm.delete(itemName);
        if (confirmed) {
            const href = this.getAttribute('href');
            if (href) window.location.href = href;
        }
    });

    // Form validation
    SW.on('submit', 'form.needs-validation', function(e) {
        if (!SW.form.validate(this)) {
            e.preventDefault();
            e.stopPropagation();
        }
    });

    // Auto-dismiss alerts
    document.querySelectorAll('.alert[data-auto-dismiss]').forEach(alert => {
        const delay = parseInt(alert.getAttribute('data-auto-dismiss')) || 5000;
        setTimeout(() => {
            const bsAlert = bootstrap.Alert.getOrCreateInstance(alert);
            bsAlert.close();
        }, delay);
    });

    // Tooltips
    document.querySelectorAll('[data-bs-toggle="tooltip"]').forEach(el => {
        new bootstrap.Tooltip(el);
    });

    // Popovers
    document.querySelectorAll('[data-bs-toggle="popover"]').forEach(el => {
        new bootstrap.Popover(el);
    });

    console.log('SmartWorkz StarterKitMVC initialized');
});

// Export to window
window.SW = SW;
