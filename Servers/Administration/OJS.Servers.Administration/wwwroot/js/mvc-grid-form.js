/* eslint-disable */
$(document).ready(() => {
    $(':checkbox').on('click', (event) => {
        if (event.target.value === 'True') {
            event.target.value = 'False';
        } else {
            event.target.value = 'True';
        }
    });

    const forms = $('form');

    forms.find('select').select2();

    forms.submit((ev) => {
        const form = $(ev.target);

        form.find('select').prop('disabled', false);

        form.find('fieldset').each((ev, fs) => {
            const selectedCheckboxes = $(fs).find('input[type=checkbox]').toArray();

            const getExpandElement = (checkbox) => {
                const expandElement = $(`#${$(checkbox).attr('expand')} :input`)[0];

                return expandElement && expandElement.name && expandElement.value
                    ? { name: expandElement.name, value: expandElement.value }
                    : null;
            };

            const result = selectedCheckboxes.map((x) => ({
                name: $(x).data('name'),
                value: $(x).data('value'),
                isChecked: $(x).is(':checked'),
                expand: getExpandElement(x),
            }));

            const input = $(`<input name="${fs.name}">`)
                .attr('type', 'hidden')
                .val(JSON.stringify(result));

            form.append(input);
        });

        return true;
    });
});
