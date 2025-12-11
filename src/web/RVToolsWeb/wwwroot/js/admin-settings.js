/**
 * Admin Settings Page JavaScript
 * Handles AJAX operations for settings management
 */
(function () {
    'use strict';

    // Toast notification helper
    function showToast(message, isError = false) {
        const toast = document.getElementById('settingsToast');
        const toastTitle = document.getElementById('toastTitle');
        const toastMessage = document.getElementById('toastMessage');
        const toastIconSuccess = document.getElementById('toastIconSuccess');
        const toastIconError = document.getElementById('toastIconError');

        toastTitle.textContent = isError ? 'Error' : 'Success';
        toastMessage.textContent = message;

        if (isError) {
            toastIconSuccess.classList.add('d-none');
            toastIconError.classList.remove('d-none');
        } else {
            toastIconSuccess.classList.remove('d-none');
            toastIconError.classList.add('d-none');
        }

        const bsToast = new bootstrap.Toast(toast);
        bsToast.show();
    }

    // Get anti-forgery token
    function getAntiForgeryToken() {
        const tokenInput = document.querySelector('input[name="__RequestVerificationToken"]');
        return tokenInput ? tokenInput.value : '';
    }

    // Generic AJAX POST helper
    async function postJson(url, data) {
        const response = await fetch(url, {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
                'RequestVerificationToken': getAntiForgeryToken()
            },
            body: JSON.stringify(data)
        });

        if (!response.ok) {
            throw new Error(`HTTP error! status: ${response.status}`);
        }

        return await response.json();
    }

    // ============================================
    // General Settings Tab
    // ============================================
    function initGeneralSettings() {
        document.querySelectorAll('.btn-save-setting').forEach(button => {
            button.addEventListener('click', async function () {
                const settingName = this.dataset.settingName;
                const row = this.closest('tr');
                const valueInput = row.querySelector('.setting-value');
                const settingValue = valueInput.value;

                try {
                    this.disabled = true;
                    this.innerHTML = '<span class="spinner-border spinner-border-sm"></span>';

                    const result = await postJson('/Settings/UpdateSetting', {
                        settingName: settingName,
                        settingValue: settingValue
                    });

                    if (result.success) {
                        showToast(`Setting "${settingName}" updated successfully`);
                    } else {
                        showToast(result.error || 'Failed to update setting', true);
                    }
                } catch (error) {
                    showToast('Error updating setting: ' + error.message, true);
                } finally {
                    this.disabled = false;
                    this.innerHTML = '<i class="bi bi-check"></i> Save';
                }
            });
        });
    }

    // ============================================
    // Table Retention Tab
    // ============================================
    function initTableRetention() {
        // Add new retention
        const newRetentionForm = document.getElementById('newRetentionForm');
        if (newRetentionForm) {
            newRetentionForm.addEventListener('submit', async function (e) {
                e.preventDefault();

                const tableSelect = document.getElementById('newRetentionTable');
                const daysInput = document.getElementById('newRetentionDays');

                try {
                    const result = await postJson('/Settings/AddRetention', {
                        fullTableName: tableSelect.value,
                        retentionDays: parseInt(daysInput.value)
                    });

                    if (result.success) {
                        showToast('Retention override added successfully');
                        // Reload page to show new entry
                        setTimeout(() => location.reload(), 1000);
                    } else {
                        showToast(result.error || 'Failed to add retention override', true);
                    }
                } catch (error) {
                    showToast('Error adding retention: ' + error.message, true);
                }
            });
        }

        // Update retention
        document.querySelectorAll('.btn-update-retention').forEach(button => {
            button.addEventListener('click', async function () {
                const id = parseInt(this.dataset.id);
                const row = this.closest('tr');
                const daysInput = row.querySelector('.retention-days');
                const retentionDays = parseInt(daysInput.value);

                try {
                    this.disabled = true;
                    this.innerHTML = '<span class="spinner-border spinner-border-sm"></span>';

                    const result = await postJson('/Settings/UpdateRetention', {
                        id: id,
                        retentionDays: retentionDays
                    });

                    if (result.success) {
                        showToast('Retention updated successfully');
                    } else {
                        showToast(result.error || 'Failed to update retention', true);
                    }
                } catch (error) {
                    showToast('Error updating retention: ' + error.message, true);
                } finally {
                    this.disabled = false;
                    this.innerHTML = '<i class="bi bi-check"></i>';
                }
            });
        });

        // Delete retention
        document.querySelectorAll('.btn-delete-retention').forEach(button => {
            button.addEventListener('click', async function () {
                const id = parseInt(this.dataset.id);

                if (!confirm('Are you sure you want to delete this retention override?')) {
                    return;
                }

                try {
                    this.disabled = true;
                    this.innerHTML = '<span class="spinner-border spinner-border-sm"></span>';

                    const result = await postJson('/Settings/DeleteRetention', {
                        id: id
                    });

                    if (result.success) {
                        showToast('Retention override deleted');
                        // Remove row from table
                        this.closest('tr').remove();
                    } else {
                        showToast(result.error || 'Failed to delete retention', true);
                    }
                } catch (error) {
                    showToast('Error deleting retention: ' + error.message, true);
                } finally {
                    this.disabled = false;
                    this.innerHTML = '<i class="bi bi-trash"></i>';
                }
            });
        });
    }

    // ============================================
    // Application Settings Tab
    // ============================================
    function initAppSettings() {
        const appSettingsForm = document.getElementById('appSettingsForm');
        if (appSettingsForm) {
            appSettingsForm.addEventListener('submit', async function (e) {
                e.preventDefault();

                const formData = new FormData(this);
                const submitBtn = this.querySelector('button[type="submit"]');

                const settings = {
                    applicationTitle: formData.get('applicationTitle'),
                    logoPath: formData.get('logoPath'),
                    filterCacheMinutes: parseInt(formData.get('filterCacheMinutes')),
                    loggingEnabled: formData.has('loggingEnabled'),
                    loggingMinimumLevel: formData.get('loggingMinimumLevel'),
                    logRetentionDays: parseInt(formData.get('logRetentionDays')),
                    databaseLoggingEnabled: formData.has('databaseLoggingEnabled'),
                    databaseLoggingMinimumLevel: formData.get('databaseLoggingMinimumLevel'),
                    consoleLoggingEnabled: formData.has('consoleLoggingEnabled'),
                    consoleLoggingMinimumLevel: formData.get('consoleLoggingMinimumLevel')
                };

                try {
                    submitBtn.disabled = true;
                    submitBtn.innerHTML = '<span class="spinner-border spinner-border-sm me-1"></span>Saving...';

                    const result = await postJson('/Settings/UpdateAppSettings', settings);

                    if (result.success) {
                        showToast('Application settings saved. Some changes may require an app restart.');
                    } else {
                        showToast(result.error || 'Failed to save application settings', true);
                    }
                } catch (error) {
                    showToast('Error saving settings: ' + error.message, true);
                } finally {
                    submitBtn.disabled = false;
                    submitBtn.innerHTML = '<i class="bi bi-check-circle me-1"></i>Save Application Settings';
                }
            });
        }
    }

    // ============================================
    // Database Status Tab
    // ============================================
    function initDatabaseStatus() {
        const refreshBtn = document.getElementById('refreshStatus');
        if (refreshBtn) {
            refreshBtn.addEventListener('click', async function () {
                try {
                    this.disabled = true;
                    this.innerHTML = '<span class="spinner-border spinner-border-sm me-1"></span>Refreshing...';

                    const response = await fetch('/Settings/RefreshStatus');
                    if (response.ok) {
                        const html = await response.text();
                        // Find the status tab pane and update its content
                        const statusPane = document.getElementById('status');
                        if (statusPane) {
                            statusPane.innerHTML = html;
                            // Re-initialize the refresh button
                            initDatabaseStatus();
                        }
                        showToast('Status refreshed');
                    } else {
                        showToast('Failed to refresh status', true);
                    }
                } catch (error) {
                    showToast('Error refreshing status: ' + error.message, true);
                } finally {
                    this.disabled = false;
                    this.innerHTML = '<i class="bi bi-arrow-clockwise me-1"></i>Refresh Status';
                }
            });
        }
    }

    // ============================================
    // Initialize all handlers on page load
    // ============================================
    document.addEventListener('DOMContentLoaded', function () {
        initGeneralSettings();
        initTableRetention();
        initAppSettings();
        initDatabaseStatus();

        // Initialize Bootstrap tooltips
        const tooltipTriggerList = [].slice.call(document.querySelectorAll('[data-bs-toggle="tooltip"]'));
        tooltipTriggerList.forEach(function (tooltipTriggerEl) {
            new bootstrap.Tooltip(tooltipTriggerEl);
        });
    });
})();
