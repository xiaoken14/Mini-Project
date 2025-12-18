$(document).ready(function() {
    // Schedule form submission
    $('.schedule-form').on('submit', function(e) {
        e.preventDefault();
        const form = $(this);
        const dayOfWeek = form.data('day');
        const submitBtn = form.find('button[type="submit"]');
        const originalText = submitBtn.html();

        // Disable button and show loading
        submitBtn.prop('disabled', true).html('<i class="fas fa-spinner fa-spin me-1"></i>Updating...');

        const formData = {
            DayOfWeek: parseInt(form.find('input[name="DayOfWeek"]').val()),
            IsAvailable: form.find('input[name="IsAvailable"]').is(':checked'),
            StartTime: form.find('input[name="StartTime"]').val() + ':00',
            EndTime: form.find('input[name="EndTime"]').val() + ':00',
            BreakStartTime: form.find('input[name="BreakStartTime"]').val() ? form.find('input[name="BreakStartTime"]').val() + ':00' : null,
            BreakEndTime: form.find('input[name="BreakEndTime"]').val() ? form.find('input[name="BreakEndTime"]').val() + ':00' : null,
            SlotDurationMinutes: parseInt(form.find('select[name="SlotDurationMinutes"]').val())
        };

        // Add CSRF token
        formData['__RequestVerificationToken'] = $('input[name="__RequestVerificationToken"]').val();

        $.ajax({
            url: '/Doctor/UpdateSchedule',
            type: 'POST',
            data: formData,
            success: function(response) {
                if (response.success) {
                    submitBtn.removeClass('btn-primary').addClass('btn-success').html('<i class="fas fa-check me-1"></i>Updated!');
                    showToast('Success', response.message, 'success');
                    updateStatistics();
                    setTimeout(function() {
                        submitBtn.removeClass('btn-success').addClass('btn-primary').html(originalText).prop('disabled', false);
                    }, 2000);
                }
            },
            error: function(xhr) {
                submitBtn.removeClass('btn-primary').addClass('btn-danger').html('<i class="fas fa-times me-1"></i>Error').prop('disabled', false);
                showToast('Error', 'Failed to update schedule. Please try again.', 'error');
                setTimeout(function() {
                    submitBtn.removeClass('btn-danger').addClass('btn-primary').html(originalText);
                }, 3000);
            }
        });
    });

    // Delete schedule
    $('.btn-delete-schedule').on('click', function() {
        const dayOfWeek = $(this).data('day');
        const dayName = $(this).data('day-name');
        
        if (confirm(`Are you sure you want to delete the schedule for ${dayName}?`)) {
            $.ajax({
                url: '/Doctor/DeleteSchedule',
                type: 'POST',
                data: { 
                    dayOfWeek: dayOfWeek,
                    '__RequestVerificationToken': $('input[name="__RequestVerificationToken"]').val()
                },
                success: function(response) {
                    if (response.success) {
                        showToast('Success', response.message, 'success');
                        location.reload();
                    } else {
                        showToast('Error', response.message, 'error');
                    }
                },
                error: function() {
                    showToast('Error', 'Failed to delete schedule', 'error');
                }
            });
        }
    });

    // Copy schedule
    let sourceDayForCopy = null;
    $('.btn-copy-schedule').on('click', function() {
        sourceDayForCopy = $(this).data('day');
        const dayNames = ['Sunday', 'Monday', 'Tuesday', 'Wednesday', 'Thursday', 'Friday', 'Saturday'];
        $('#sourceDayName').text(dayNames[sourceDayForCopy]);
        
        // Uncheck all and disable source day
        $('#copyScheduleModal input[type="checkbox"]').prop('checked', false).prop('disabled', false);
        $(`#copyScheduleModal input[value="${sourceDayForCopy}"]`).prop('disabled', true);
        $('#copyScheduleModal').modal('show');
    });

    // Confirm copy schedule
    $('#confirmCopySchedule').on('click', function() {
        const targetDays = [];
        $('#copyScheduleModal input[type="checkbox"]:checked').each(function() {
            targetDays.push(parseInt($(this).val()));
        });

        if (targetDays.length === 0) {
            showToast('Warning', 'Please select at least one day to copy to', 'error');
            return;
        }

        $.ajax({
            url: '/Doctor/CopySchedule',
            type: 'POST',
            data: { 
                sourceDayOfWeek: sourceDayForCopy,
                targetDays: targetDays,
                '__RequestVerificationToken': $('input[name="__RequestVerificationToken"]').val()
            },
            success: function(response) {
                if (response.success) {
                    showToast('Success', response.message, 'success');
                    $('#copyScheduleModal').modal('hide');
                    location.reload();
                } else {
                    showToast('Error', response.message, 'error');
                }
            },
            error: function() {
                showToast('Error', 'Failed to copy schedule', 'error');
            }
        });
    });

    // Preset buttons
    $('.btn-preset').on('click', function() {
        const preset = $(this).data('preset');
        const form = $(this).closest('.schedule-form');
        
        switch(preset) {
            case 'morning':
                form.find('input[name="StartTime"]').val('09:00');
                form.find('input[name="EndTime"]').val('12:00');
                form.find('input[name="BreakStartTime"]').val('');
                form.find('input[name="BreakEndTime"]').val('');
                break;
            case 'afternoon':
                form.find('input[name="StartTime"]').val('14:00');
                form.find('input[name="EndTime"]').val('18:00');
                form.find('input[name="BreakStartTime"]').val('');
                form.find('input[name="BreakEndTime"]').val('');
                break;
            case 'fullday':
                form.find('input[name="StartTime"]').val('09:00');
                form.find('input[name="EndTime"]').val('17:00');
                form.find('input[name="BreakStartTime"]').val('12:00');
                form.find('input[name="BreakEndTime"]').val('13:00');
                break;
        }
        form.find('input[name="IsAvailable"]').prop('checked', true).trigger('change');
    });

    // Save template
    $('#saveTemplateForm').on('submit', function(e) {
        e.preventDefault();
        const templateName = $(this).find('input[name="templateName"]').val();
        const description = $(this).find('textarea[name="description"]').val();

        if (!templateName.trim()) {
            showToast('Error', 'Template name is required', 'error');
            return;
        }

        $.ajax({
            url: '/Doctor/SaveTemplate',
            type: 'POST',
            data: { 
                templateName: templateName,
                description: description,
                '__RequestVerificationToken': $('input[name="__RequestVerificationToken"]').val()
            },
            success: function(response) {
                if (response.success) {
                    showToast('Success', response.message, 'success');
                    $('#templateModal').modal('hide');
                    $('#saveTemplateForm')[0].reset();
                } else {
                    showToast('Error', response.message || 'Failed to save template', 'error');
                }
            },
            error: function(xhr) {
                var errorMessage = 'Failed to save template';
                if (xhr.responseJSON && xhr.responseJSON.message) {
                    errorMessage = xhr.responseJSON.message;
                }
                showToast('Error', errorMessage, 'error');
            }
        });
    });

    // Load templates
    $('#loadTemplateModal').on('show.bs.modal', function() {
        loadTemplates();
    });

    // Toggle form fields based on availability
    $('input[name="IsAvailable"]').on('change', function() {
        const form = $(this).closest('.schedule-form');
        const isAvailable = $(this).is(':checked');
        const label = form.find('.availability-label');
        const card = form.closest('.card');
        const header = card.find('.card-header');

        // Toggle form fields
        form.find('input[type="time"], select').prop('disabled', !isAvailable);

        // Toggle text and styling
        if (!isAvailable) {
            card.addClass('opacity-50').addClass('unavailable-card');
            label.find('.available-text').hide();
            label.find('.unavailable-text').show();
            label.removeClass('text-success').addClass('text-danger');
            
            // Update header badge
            header.find('.badge-success').removeClass('badge-success bg-success').addClass('badge-danger bg-danger').text('Unavailable');
            header.find('small.text-muted').text('No appointments will be accepted');
        } else {
            card.removeClass('opacity-50').removeClass('unavailable-card');
            label.find('.available-text').show();
            label.find('.unavailable-text').hide();
            label.removeClass('text-danger').addClass('text-success');
            
            // Update header badge
            header.find('.badge-danger').removeClass('badge-danger bg-danger').addClass('badge-success bg-success').text('Available');
            // Note: Time display will be updated when form is saved
        }
    });

    // Clear all schedules
    $('#clearAllBtn').on('click', function() {
        if (confirm('⚠️ WARNING: This will permanently delete ALL your schedules, templates, and special schedules.\n\nThis action cannot be undone. Are you sure you want to continue?')) {
            if (confirm('Are you absolutely sure? This will clear everything and you will start with a blank schedule.')) {
                $.ajax({
                    url: '/Doctor/ClearAllSchedules',
                    type: 'POST',
                    data: { 
                        '__RequestVerificationToken': $('input[name="__RequestVerificationToken"]').val()
                    },
                    success: function(response) {
                        if (response.success) {
                            showToast('Success', response.message, 'success');
                            setTimeout(function() {
                                location.reload();
                            }, 2000);
                        } else {
                            showToast('Error', response.message, 'error');
                        }
                    },
                    error: function() {
                        showToast('Error', 'Failed to clear schedules', 'error');
                    }
                });
            }
        }
    });

    // Initialize form states
    $('input[name="IsAvailable"]').trigger('change');
});

function loadTemplates() {
    $.ajax({
        url: '/Doctor/GetTemplates',
        type: 'GET',
        success: function(response) {
            let html = '';
            
            // Handle both array response and error response
            if (response.success === false) {
                html = '<div class="text-center text-danger"><p>' + (response.message || 'Failed to load templates') + '</p></div>';
            } else {
                const templates = Array.isArray(response) ? response : [];
                if (templates.length === 0) {
                    html = '<div class="text-center text-muted"><p>No templates saved yet.</p><p class="small">Create some schedules first, then save them as a template.</p></div>';
                } else {
                    templates.forEach(function(template) {
                        html += `<div class="card mb-2">
                            <div class="card-body">
                                <div class="d-flex justify-content-between align-items-start">
                                    <div>
                                        <h6 class="card-title mb-1">${template.name || 'Unnamed Template'}</h6>
                                        <p class="card-text small text-muted">${template.description || 'No description'}</p>
                                    </div>
                                    <button class="btn btn-primary btn-sm" onclick="applyTemplate(${template.id})">
                                        <i class="fas fa-download me-1"></i>Apply
                                    </button>
                                </div>
                            </div>
                        </div>`;
                    });
                }
            }
            $('#templatesList').html(html);
        },
        error: function(xhr) {
            var errorMessage = 'Failed to load templates';
            if (xhr.responseJSON && xhr.responseJSON.message) {
                errorMessage = xhr.responseJSON.message;
            }
            $('#templatesList').html('<div class="text-center text-danger"><p>' + errorMessage + '</p></div>');
        }
    });
}

function applyTemplate(templateId) {
    if (confirm('This will replace your current schedule. Are you sure?')) {
        $.ajax({
            url: '/Doctor/ApplyTemplate',
            type: 'POST',
            data: { 
                templateId: templateId,
                '__RequestVerificationToken': $('input[name="__RequestVerificationToken"]').val()
            },
            success: function(response) {
                if (response.success) {
                    showToast('Success', response.message, 'success');
                    $('#loadTemplateModal').modal('hide');
                    location.reload();
                } else {
                    showToast('Error', response.message || 'Failed to apply template', 'error');
                }
            },
            error: function(xhr) {
                var errorMessage = 'Failed to apply template';
                if (xhr.responseJSON && xhr.responseJSON.message) {
                    errorMessage = xhr.responseJSON.message;
                }
                showToast('Error', errorMessage, 'error');
            }
        });
    }
}

function updateStatistics() {
    // This would typically make an AJAX call to get updated stats
    // For now, we'll just reload the page to show updated stats
    setTimeout(function() {
        location.reload();
    }, 2500);
}

function showToast(title, message, type) {
    // Simple toast notification - you can replace with your preferred toast library
    const alertClass = type === 'success' ? 'alert-success' : 'alert-danger';
    const toast = $(`<div class="alert ${alertClass} alert-dismissible fade show position-fixed" style="top: 20px; right: 20px; z-index: 9999; min-width: 300px;">
        <strong>${title}:</strong> ${message}
        <button type="button" class="btn-close" data-bs-dismiss="alert"></button>
    </div>`);
    
    $('body').append(toast);
    setTimeout(function() {
        toast.alert('close');
    }, 5000);
}