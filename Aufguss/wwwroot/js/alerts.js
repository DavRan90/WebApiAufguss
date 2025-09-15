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
function showCreateEventAlert(event) {
    const startDate = new Date(event.start);
    const dateString = startDate.toISOString().split('T')[0]; // yyyy-mm-dd
    const timeString = startDate.toTimeString().slice(0, 5);   // HH:mm

    return Swal.fire({
        title: 'Lägg till pass',
        html:
            `<input id="swal-title" class="swal2-input" value="${event.title || ''}" placeholder="Titel">` +
            `<select id="swal-gender" class="swal2-input">
            <option value="Man" ${event.gender === 'Man' ? 'selected' : ''}>Herrar</option>
            <option value="Woman" ${event.gender === 'Woman' ? 'selected' : ''}>Damer</option>
            </select>` +
            `<input id="swal-date" class="swal2-input" value="${dateString}" readonly style="background-color: #f8f9fa; cursor: not-allowed;">` +
            `<input id="swal-time" type="time" class="swal2-input" value="${timeString}">` +
            `<textarea style="height: 200px; padding-top: 0.5em" id="swal-description" class="swal2-input" placeholder="Beskrivning">${event.description || ''}</textarea>` +
            `<input id="swal-maxSlots" type="number" class="swal2-input" value="${event.maxSlots || ''}">` +
            `<div style="margin-top: 1rem; text-align: center;">` +
            `<label><input type="checkbox" id="swal-hidden" ${event.hidden ? 'checked' : ''}> Dold</label>` +
            `</div>`,
        focusConfirm: false,
        showCancelButton: true,
        confirmButtonText: 'Skapa',
        preConfirm: () => {
            const title = document.getElementById('swal-title').value;
            const description = document.getElementById('swal-description').value;
            const date = document.getElementById('swal-date').value;
            const time = document.getElementById('swal-time').value;
            const maxSlots = parseInt(document.getElementById('swal-maxSlots').value, 10);
            const hidden = document.getElementById('swal-hidden').checked;
            const gender = document.getElementById('swal-gender').value;

            if (!title || !description || !date || !time || isNaN(maxSlots)) {
                Swal.showValidationMessage('Alla fält behöver fyllas i korrekt');
                return false;
            }

            // Combine date and time into ISO format (e.g. 2025-09-15T14:00:00)
            const start = `${date}T${time}:00`;

            return JSON.stringify({
                title: title,
                description: description,
                start: start,
                maxSlots: maxSlots,
                hidden: hidden,
                gender: gender
            });
        }
    }).then(result => {
        if (result.isConfirmed) {
            return result.value;
        }
        return null;
    });
}

function showEventForm() {
    return Swal.fire({
        title: 'Lägg till pass',
        html:
            `<input id="swal-title" class="swal2-input" placeholder="Titel">` +
            `<textarea style="height: 200px; padding-top: 0.5em" id="swal-description" class="swal2-input" placeholder="Beskrivning"></textarea>` +
            `<input id="swal-maxSlots" type="number" class="swal2-input">`,
        focusConfirm: false,
        showCancelButton: true,
        preConfirm: () => {
            const title = document.getElementById('swal-title').value;
            const description = document.getElementById('swal-description').value;
            const maxSlots = parseInt(document.getElementById('swal-maxSlots').value, 10);

            if (!title || !description || !maxSlots) {
                Swal.showValidationMessage('Fyll i alla fält');
                return false;
            }

            return JSON.stringify({
                title: title,
                description: description,
                maxSlots: maxSlots
            });
        }
    }).then(result => {
        if (result.isConfirmed) {
            return result.value;
        }
        return null;
    });
}