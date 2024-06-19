// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.

jQuery('document').ready(function ($) {
    $('.hero-item-wrap').slick(
        {
            dots: true,
            autoplay: true
        }
    );
})
