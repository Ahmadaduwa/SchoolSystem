const purgecss = require('@fullhuman/postcss-purgecss').default;

module.exports = {
    plugins: [
        require('autoprefixer'),
        purgecss({
            content: [
                './Views/**/*.cshtml',
                './Views/Shared/**/*.cshtml',
                './wwwroot/**/*.js',
                './wwwroot/**/*.html',
            ],
            safelist: [], // Classes to keep
            defaultExtractor: content => content.match(/[\w-/:]+(?<!:)/g) || [],
        }),
    ],
};
