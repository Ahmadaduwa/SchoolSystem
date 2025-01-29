document.addEventListener("DOMContentLoaded", function () {
    const dropdownToggles = document.querySelectorAll(".table-dropdown-toggle");

    dropdownToggles.forEach((toggle) => {
        toggle.addEventListener("click", function (event) {
            event.stopPropagation();  // ป้องกันไม่ให้คลิกนอก dropdown ทำให้มันปิด
            event.preventDefault();

            const dropdownId = this.getAttribute("data-dropdown");
            const dropdownMenu = document.getElementById(dropdownId);

            // ซ่อนเมนู dropdown อื่น ๆ
            document.querySelectorAll(".table-dropdown-menu").forEach(menu => {
                if (menu !== dropdownMenu) {
                    menu.classList.add("hidden");
                }
            });

            // แสดง/ซ่อนเมนู dropdown ที่คลิก
            dropdownMenu.classList.toggle("hidden");

            // ตรวจสอบว่า dropdown ไม่ล้นขอบจอ
            const rect = dropdownMenu.getBoundingClientRect();
            if (rect.bottom > window.innerHeight) {
                dropdownMenu.style.top = "auto";
                dropdownMenu.style.bottom = "100%";
            } else {
                dropdownMenu.style.top = "100%";
                dropdownMenu.style.bottom = "auto";
            }
        });
    });

    // ปิด dropdown เมื่อคลิกข้างนอก
    document.addEventListener("click", function () {
        document.querySelectorAll(".table-dropdown-menu").forEach(menu => {
            menu.classList.add("hidden");
        });
    });
});
