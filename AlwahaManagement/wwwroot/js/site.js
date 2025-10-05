// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

function FetchPartial(url, model){
    fetch(url).then(data => {return data.text()}).then(body => {
        model.innerHTML = body;
        var scripts = model.querySelectorAll('script');
        for (let i = 0; i < scripts.length; i++) {
            if (scripts[i].type !== "text/x-template") {
                eval(scripts[i].innerHTML);
            }
        }
    });
}

// Add shadow to navbar on scroll
document.addEventListener('DOMContentLoaded', function() {
    const navbar = document.querySelector('.navbar');

    if (navbar) {
        window.addEventListener('scroll', function() {
            if (window.scrollY > 10) {
                navbar.classList.add('scrolled');
            } else {
                navbar.classList.remove('scrolled');
            }
        });
    }

    // Add smooth hover effect to dropdown items
    const dropdownItems = document.querySelectorAll('.dropdown-item');
    dropdownItems.forEach(item => {
        item.addEventListener('mouseenter', function() {
            this.style.transition = 'all 0.2s ease';
        });
    });
});


function confirmDelete(name, url, modelId, partialUrl){
    Swal.fire({
        title: 'Are you sure you want to delete ' + name + '?',
        icon: 'warning',
        showCancelButton: true,
        confirmButtonColor: '#3085d6',
        cancelButtonColor: '#d33',
        confirmButtonText: 'Yes'
    }).then((res) => {
        if(res.value){
            $.ajax({
                method: "POST",
                url: url,
                success: function (r){
                    if(partialUrl === undefined){
                        location.reload();
                    }
                    else{
                        FetchPartial(partialUrl, document.getElementById(modelId));
                    }
                }
            })
        }
    })
}

function confirmRestore(name, url, modelId, partialUrl){
    Swal.fire({
        title: 'Are you sure you want to restore ' + name + '?',
        icon: 'question',
        showCancelButton: true,
        confirmButtonColor: '#10b981',
        cancelButtonColor: '#d33',
        confirmButtonText: 'Yes, Restore'
    }).then((res) => {
        if(res.value){
            $.ajax({
                method: "POST",
                url: url,
                success: function (r){
                    if(partialUrl === undefined){
                        location.reload();
                    }
                    else{
                        FetchPartial(partialUrl, document.getElementById(modelId));
                    }
                }
            })
        }
    })
}

function toggleAvailability(itemId, isAvailable){
    $.ajax({
        method: "POST",
        url: '/Menu/ToggleAvailable?id=' + itemId + '&isAvailable=' + isAvailable,
        success: function (r){
            // Update header styling
            var header = $('#header-' + itemId);
            var title = header.find('.card-title');
            var badge = header.find('.badge');
            var label = $('#label-' + itemId);

            if (isAvailable) {
                // Making available
                header.css('background', 'linear-gradient(135deg, #6366f1 0%, #8b5cf6 100%)');
                title.removeClass('fst-italic');
                badge.removeClass('fst-italic');
                label.text('Available');
            } else {
                // Making unavailable
                header.css('background', 'linear-gradient(135deg, #6c757d 0%, #868e96 100%)');
                title.addClass('fst-italic');
                badge.addClass('fst-italic');
                label.text('Unavailable');
            }

            // Update the onchange handler for next toggle
            var toggle = $('#toggle-' + itemId);
            toggle.attr('onchange', 'toggleAvailability("' + itemId + '", ' + !isAvailable + ')');
        },
        error: function(xhr, status, error) {
            alert('Error toggling availability: ' + error);
            // Revert the checkbox state on error
            var toggle = $('#toggle-' + itemId);
            toggle.prop('checked', !isAvailable);
        }
    })
}

function toggleSetAvailability(setId, isAvailable){
    $.ajax({
        method: "POST",
        url: '/Menu/ToggleSetAvailability?id=' + setId + '&isAvailable=' + isAvailable,
        success: function (r){
            // Update header styling
            var header = $('#header-' + setId);
            var title = header.find('.card-title');
            var label = $('#label-' + setId);

            if (isAvailable) {
                // Making available - restore original color from data attribute
                var originalColor = header.data('original-color');
                // Store the current font color if not already stored, then restore it
                if (!header.data('original-font-color')) {
                    header.data('original-font-color', header.css('color'));
                }
                var originalFontColor = header.data('original-font-color');
                header.css('background-color', originalColor);
                header.css('color', originalFontColor);
                title.removeClass('fst-italic');
                label.text('Available');
            } else {
                // Making unavailable - set to grey
                // Store original font color before changing
                if (!header.data('original-font-color')) {
                    header.data('original-font-color', header.css('color'));
                }
                header.css('background-color', '#6c757d');
                header.css('color', '#ffffff');
                title.addClass('fst-italic');
                label.text('Unavailable');
            }

            // Update the onchange handler for next toggle
            var toggle = $('#toggle-' + setId);
            toggle.attr('onchange', 'toggleSetAvailability("' + setId + '", ' + !isAvailable + ')');
        },
        error: function(xhr, status, error) {
            alert('Error toggling availability: ' + error);
            // Revert the checkbox state on error
            var toggle = $('#toggle-' + setId);
            toggle.prop('checked', !isAvailable);
        }
    })
}



