function showFormAlert() {
    return Swal.fire({
        title: 'User Info',
        html:
            `<input id="swal-name" class="swal2-input" placeholder="Name">` +
            `<input id="swal-email" class="swal2-input" placeholder="Email">` +
            `<input id="swal-tel" class="swal2-input" placeholder="Tel">`,
        focusConfirm: false,
        showCancelButton: true,
        preConfirm: () => {
            const name = document.getElementById('swal-name').value;
            const email = document.getElementById('swal-email').value;
            const tel = document.getElementById('swal-tel').value;

            if (!name || !email || !tel) {
                Swal.showValidationMessage('Both fields required');
                return false;
            }

            return { name, email, tel};
        }
    });
}

function showBookingForm(eventId) {
    return Swal.fire({
        title: 'Lägg till deltagare',
        html:
            `<input id="booking-name" class="swal2-input" placeholder="Namn">` +
            `<input id="booking-email" class="swal2-input" placeholder="Email">` +
            `<input id="swal-tel" class="swal2-input" placeholder="Tel">`,
        focusConfirm: false,
        showCancelButton: true,
        preConfirm: () => {
            const name = document.getElementById('booking-name').value;
            const email = document.getElementById('booking-email').value;
            const tel = document.getElementById('swal-tel').value;

            if (!name || !email) {
                Swal.showValidationMessage('Please enter name, email and telephone');
                return false;
            }

            return JSON.stringify({
                name: name,
                email: email,
                tel: tel,
                eventId: eventId
            });
        }
    }).then(result => {
        if (result.isConfirmed) {
            return result.value;
        }
        return null;
    });
}