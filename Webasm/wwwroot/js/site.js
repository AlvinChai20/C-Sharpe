document.addEventListener("DOMContentLoaded", function () {
    const modal = document.getElementById("detailsModal");
    const modalTitle = document.getElementById("modalTitle");
    const modalDescription = document.getElementById("modalDescription");
    const modalPrice = document.getElementById("modalPrice");
    const closeBtn = document.querySelector(".modal .close");

    // Mapping of service to descriptions + price
    const serviceDetails = {
        Scaling: {
            title: "Dental Scaling",
            desc: "Scaling removes plaque and tartar from teeth and below the gums. Recommended every 6 months.",
            price: "RM150"
        },
        Filling: {
            title: "Dental Filling",
            desc: "Fillings repair cavities or cracks in teeth using composite or metal materials.",
            price: "RM200"
        },
        Braces: {
            title: "Braces",
            desc: "Braces straighten misaligned teeth using gentle, continuous pressure.",
            price: "RM5000"
        },
        Whitening: {
            title: "Teeth Whitening",
            desc: "Whitening brightens your smile by removing stains using safe bleaching agents.",
            price: "RM400"
        }
    };


    // Function to bind click events
    function attachImageClickEvents() {
        document.querySelectorAll(".image-container").forEach(container => {
            container.addEventListener("click", function () {
                const service = this.getAttribute("data-service");
                if (serviceDetails[service]) {
                    modalTitle.innerText = serviceDetails[service].title;
                    modalDescription.innerText = serviceDetails[service].desc;
                    modalPrice.innerText = "Price: " + serviceDetails[service].price;
                    modal.style.display = "flex";

                    document.getElementById("proceedBooking").setAttribute(
                        "href",
                        `/Booking/Index?service=${encodeURIComponent(serviceDetails[service].title)}&price=${encodeURIComponent(serviceDetails[service].price)}`
                    );
                }
            });
        });
    }

    // Initial bind
    attachImageClickEvents();

    // Close modal when clicking X
    closeBtn.addEventListener("click", function () {
        modal.style.display = "none";
    });

    // Close modal when clicking outside
    window.addEventListener("click", function (event) {
        if (event.target === modal) {
            modal.style.display = "none";
        }
    });

    // 🔍 Search box logic
    document.getElementById("searchBox").addEventListener("keyup", function () {
        var query = this.value;
        fetch("/Appointments/Search?query=" + encodeURIComponent(query))
            .then(response => response.text())
            .then(html => {
                document.getElementById("treatmentList").innerHTML = html;
                attachImageClickEvents(); // re-bind events after search results load
            });
    });

    // Booking redirect
    window.proceedToBooking = function () {
        window.location.href = "/Appointments/Create"; // Adjust if your route is different
    }

    $(document).ready(function () {
        function loadTreatments() {
            var query = $("#searchBox").val();
            var sortOrder = $("#sortBox").val();

            $.ajax({
                url: '/Appointments/Search',
                type: 'GET',
                data: { query: query, sortOrder: sortOrder },
                success: function (result) {
                    $("#treatmentList").html(result);
                }
            });
        }

        // 🔍 Trigger search
        $("#searchBox").on("input", function () {
            loadTreatments();
        });

        // 🔽 Trigger sort
        $("#sortBox").on("change", function () {
            loadTreatments();
        });
    });

    // Open modal
    function openModal() {
        const modal = document.getElementById("myModal");
        modal.classList.add("show");
        document.body.style.overflow = "hidden"; // disable background scroll
    }

    // Close modal
    function closeModal() {
        const modal = document.getElementById("myModal");
        modal.classList.remove("show");
        document.body.style.overflow = ""; // restore background scroll
    }



    // Example when showing modal
    function showServiceDetails(serviceKey) {
        const details = serviceDetails[serviceKey];
        modalTitle.textContent = details.title;
        modalDescription.textContent = details.desc;
        modalPrice.textContent = details.price;
        modal.style.display = "block";
    }

    function loadAppointments(page = 1) {
        var search = $("#searchBox").val();
        var userId = $("#userFilter").val();
        var pageSize = $("#pageSize").val();

        $.get("/Booking/AppointmentHistory", { search: search, userId: userId, page: page, pageSize: pageSize }, function (data) {
            $("#appointmentsTable").html($(data).find("#appointmentsTable").html());
            $(".pagination").html($(data).find(".pagination").html());
        });
    }

    // 🔄 Reload when page size changes
    $("#pageSize").change(function () {
        loadAppointments(1); // reset to page 1
    });


});
