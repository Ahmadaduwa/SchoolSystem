module.exports = {
    purge: {
        content: ['./Views/**/*.cshtml', './wwwroot/js/**/*.js'], // ปรับเส้นทางไฟล์ที่ต้องการ purge
    },
    darkMode: false, // หรือตั้งเป็น media หากต้องการ
    theme: {
        extend: {},
    },
    variants: {
        extend: {},
    },
    plugins: [],
};
