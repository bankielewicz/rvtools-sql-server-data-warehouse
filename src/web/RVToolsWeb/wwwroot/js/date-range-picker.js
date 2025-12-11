/**
 * Date Range Picker initialization for trend reports
 * Uses flatpickr for date selection with quick-select buttons
 */
(function () {
    'use strict';

    var fpStart = null;
    var fpEnd = null;

    /**
     * Initialize date range pickers on page load
     */
    function initDateRangePickers() {
        var startDateInput = document.getElementById('startDate');
        var endDateInput = document.getElementById('endDate');

        if (!startDateInput || !endDateInput) {
            return;
        }

        // Initialize start date picker
        fpStart = flatpickr(startDateInput, {
            dateFormat: 'Y-m-d',
            maxDate: 'today',
            allowInput: false,
            onChange: function (selectedDates) {
                if (selectedDates[0] && fpEnd) {
                    fpEnd.set('minDate', selectedDates[0]);
                }
                updateQuickRangeButtons();
            }
        });

        // Initialize end date picker
        fpEnd = flatpickr(endDateInput, {
            dateFormat: 'Y-m-d',
            maxDate: 'today',
            allowInput: false,
            onChange: function (selectedDates) {
                if (selectedDates[0] && fpStart) {
                    fpStart.set('maxDate', selectedDates[0]);
                }
                updateQuickRangeButtons();
            }
        });

        // Set initial constraints
        if (fpStart.selectedDates[0]) {
            fpEnd.set('minDate', fpStart.selectedDates[0]);
        }
        if (fpEnd.selectedDates[0]) {
            fpStart.set('maxDate', fpEnd.selectedDates[0]);
        }

        // Bind quick range button handlers
        document.querySelectorAll('.quick-range').forEach(function (btn) {
            btn.addEventListener('click', function () {
                var days = parseInt(this.dataset.days, 10);
                setDateRange(days);
            });
        });

        // Initial update of button states
        updateQuickRangeButtons();
    }

    /**
     * Set the date range to the specified number of days back from today
     * @param {number} days - Number of days to look back
     */
    function setDateRange(days) {
        if (!fpStart || !fpEnd) return;

        var endDate = new Date();
        endDate.setHours(0, 0, 0, 0);

        var startDate = new Date();
        startDate.setHours(0, 0, 0, 0);
        startDate.setDate(startDate.getDate() - days);

        // Temporarily remove constraints to allow setting any date
        fpStart.set('maxDate', 'today');
        fpEnd.set('minDate', null);

        // Set the dates
        fpStart.setDate(startDate, false);
        fpEnd.setDate(endDate, false);

        // Restore constraints
        fpEnd.set('minDate', startDate);
        fpStart.set('maxDate', endDate);

        updateQuickRangeButtons();
    }

    /**
     * Update the active state on quick range buttons based on current selection
     */
    function updateQuickRangeButtons() {
        if (!fpStart || !fpEnd) return;

        var start = fpStart.selectedDates[0];
        var end = fpEnd.selectedDates[0];

        if (!start || !end) return;

        // Calculate days difference
        var daysDiff = Math.round((end - start) / (1000 * 60 * 60 * 24));

        // Check if end date is today
        var today = new Date();
        today.setHours(0, 0, 0, 0);
        var endNormalized = new Date(end);
        endNormalized.setHours(0, 0, 0, 0);
        var isEndToday = endNormalized.getTime() === today.getTime();

        // Update button states
        document.querySelectorAll('.quick-range').forEach(function (btn) {
            var btnDays = parseInt(btn.dataset.days, 10);
            var isActive = isEndToday && daysDiff === btnDays;
            btn.classList.toggle('active', isActive);
        });
    }

    // Initialize when DOM is ready
    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', initDateRangePickers);
    } else {
        initDateRangePickers();
    }
})();
