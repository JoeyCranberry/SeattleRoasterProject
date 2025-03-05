/** @type {import('tailwindcss').Config} */

const plugin = require('tailwindcss/plugin')

module.exports = {
    content: ['../**/*.{razor,html,cshtml}'],
    theme: {
        extend: {
            colors: {
                
            }
        },
        fontFamily: {
            'display': ['Enriqueta'],
        },
    }
}

