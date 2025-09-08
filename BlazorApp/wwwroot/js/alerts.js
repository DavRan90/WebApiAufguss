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