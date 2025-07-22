/** @type {import('tailwindcss').Config} */

const plugin = require('tailwindcss/plugin')

module.exports = {
    content: ['../**/*.{razor,html,cshtml}'],
    theme: {
        extend: {
            colors: {
                'dark-roast': {
                    DEFAULT: '#9C6C38',
                    50: '#E1C7AB',
                    100: '#DBBD9C',
                    200: '#D1A97E',
                    300: '#C69560',
                    400: '#BA8143',
                    500: '#9C6C38',
                    600: '#7A552C',
                    700: '#583D20',
                    800: '#372614',
                    900: '#150E08',
                    950: '#040301'
                },
                'crema': {
                    DEFAULT: '#CFB9AD',
                    50: '#F9F7F5',
                    100: '#F5F0ED',
                    200: '#EBE2DD',
                    300: '#E2D4CD',
                    400: '#D8C7BD',
                    500: '#CFB9AD',
                    600: '#BA9B8A',
                    700: '#A67D66',
                    800: '#86624E',
                    900: '#624839',
                    950: '#503B2F'
                },
                'light-roast': {
                    DEFAULT: '#6F4732',
                    50: '#CCA38D',
                    100: '#C5977F',
                    200: '#B98063',
                    300: '#A76B4B',
                    400: '#8B593F',
                    500: '#6F4732',
                    600: '#5A3A28',
                    700: '#452C1F',
                    800: '#301F15',
                    900: '#1B110C',
                    950: '#100A07'
                },
                'green-coffee': {
                    DEFAULT: '#B6B966',
                    50: '#F2F3E4',
                    100: '#EBECD6',
                    200: '#DEDFBA',
                    300: '#D1D39E',
                    400: '#C3C682',
                    500: '#B6B966',
                    600: '#9B9E48',
                    700: '#767837',
                    800: '#505125',
                    900: '#2A2B14',
                    950: '#17180B'
                },
                'medium-roast': {
                    DEFAULT: '#A24501',
                    50: '#FED0AE',
                    100: '#FEC499',
                    200: '#FEAD71',
                    300: '#FE9548',
                    400: '#FE7D20',
                    500: '#F36802',
                    600: '#CB5601',
                    700: '#A24501',
                    800: '#6A2D01',
                    900: '#321600',
                    950: '#170A00'
                }, 
                'heat': "var(--theme-heat)"
            }
        },
        fontFamily: {
            'sans': ['"Josefin Sans"'],
        },
        screens: {
            'sm': '640px',
            'md': '768px',
            'lg': '1024px',
            'xl': '1280px',
            '2xl': '1536px',
            '3xl': '1850px'
        }
    },
    plugins: [plugin(function ({ addComponents, theme }) {
        addComponents({
            '.nav-link.active': {
                backgroundColor: theme('colors.green-coffee.500'),
            },
            '.nav-link.active:hover': {
                backgroundColor: theme('colors.green-coffee.600'),
            }
        })
    })],
}

