/** @type {import('tailwindcss').Config} */

const plugin = require('tailwindcss/plugin')

module.exports = {
    content: ['./**/*.{razor,html,cshtml}'],
    theme: {
        extend: {
            colors: {
                'foam': {
                    100: '#fdfaf9',
                    200: '#fbf5f2',
                    300: '#faf1ec',
                    400: '#f8ece5',
                    500: '#06b6d4',
                    600: '#b7aba5',
                    700: '#7b736f',
                    800: '#45403d',
                    900: '#151312',
                },
                'dark-roast': {
                    50: '#E1C7AB',
                    100: '#DBBD9C',
                    200: '#D0A97E',
                    300: '#C69560',
                    400: '#BA8143',
                    500: '#9C6C38',
                    600: '#7E572D',
                    700: '#604323',
                    800: '#422E18',
                    900: '#24190D',
                    950: '#0F0B06'
                },
                'crema': {
                    100: '#f1eae6',
                    200: '#e7dcd5',
                    300: '#ddcec5',
                    400: '#d9c7bd',
                    500: '#cfb9ad',
                    600: '#99897f',
                    700: '#7f716a',
                    800: '#4f4641',
                    900: '#231e1b',
                },
                'light-roast': {
                    100: '#d3c4bd',
                    200: '#c4b1a8',
                    300: '#a88d7f',
                    400: '#8b6958',
                    500: '#6f4732',
                    600: '#513222',
                    700: '#42281b',
                    800: '#27160d',
                    900: '#0e0603',
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

