function showFormAlert(booking) {
    return Swal.fire({
        title: 'Ändra deltagare',
        html:
            `<input id="swal-name" class="swal2-input" value="${booking.name || ''}" placeholder="Name">` +
            `<input id="swal-email" class="swal2-input" value="${booking.email || ''}" placeholder="Email">` +
            `<input id="swal-tel" class="swal2-input" value="${booking.tel || ''}" placeholder="Tel">` +
            `<div style="margin-top: 1rem; text-align: left;">` +
            `<label><input type="checkbox" id="swal-unbooked" ${booking.unbooked ? 'checked' : ''}> Avbokad</label>` +
            `</div>`,
        focusConfirm: false,
        showCancelButton: true,
        confirmButtonText: 'Submit',
        preConfirm: () => {
            const name = document.getElementById('swal-name').value;
            const email = document.getElementById('swal-email').value;
            const tel = document.getElementById('swal-tel').value;
            const unbooked = document.getElementById('swal-unbooked').checked;

            if (!name || !email || !tel) {
                Swal.showValidationMessage('Alla fält behöver fyllas i');
                return false;
            }

            return JSON.stringify({
                name: name,
                email: email,
                tel: tel,
                unbooked: unbooked
            });
        }
    }).then(result => {
        if (result.isConfirmed) {
            return result.value;
        }
        return null;

    });
}

function showFaqAlert(faq) {
    return Swal.fire({
        title: 'Ändra FAQ',
        html:
            `<input id="swal-question" class="swal2-input" value="${faq.question || ''}" placeholder="Question">` +
            `<textarea style="height: 200px; padding-top: 0.5em" id="swal-answer" class="swal2-input" placeholder="Answer">${faq.answer || ''}</textarea>`,
        focusConfirm: false,
        showCancelButton: true,
        confirmButtonText: 'Submit',
        preConfirm: () => {
            const question = document.getElementById('swal-question').value;
            const answer = document.getElementById('swal-answer').value;

            if (!question || !answer) {
                Swal.showValidationMessage('Alla fält behöver fyllas i');
                return false;
            }

            return JSON.stringify({
                question: question,
                answer: answer,
            });
        }
    }).then(result => {
        if (result.isConfirmed) {
            return result.value;
        }
        return null;

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
                Swal.showValidationMessage('Fyll i namn, email och telefonnummer');
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