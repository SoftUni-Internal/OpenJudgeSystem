(() => {
    $(document).ready(function () {
        const submissionTypesCheckboxes = $("input[name^='SubmissionTypes'][name$='.IsChecked']");

        submissionTypesCheckboxes.change((ev) => {
            try {
                console.log("HERE")

                const index = getElementIndex(ev.currentTarget);
                if (isChecked($(ev.currentTarget))) {
                    insertSkeleton(index, ev);
                } else {
                    removeSkeleton(index);
                }
            } catch (ex) {
                console.log(ex);
            }
        });
    });

    const getSolutionSkeletonElement = (index) => {
        return `<div class="row">
        <div class="editor-label col-xs-1">
            <div class="pull-right">
                <label for="SubmissionTypes_${index}__SolutionSkeleton">Solution skeleton</label>
            </div>
        </div>
        <div class="editor-field col-xs-6">
            <textarea class="form-control valid" cols="20" id="SubmissionTypes_${index}__SolutionSkeleton" name="SubmissionTypes[${index}].SolutionSkeleton" rows="10" spellcheck="true" aria-invalid="false"></textarea>
        </div>
         <div class="editor-field col-xs-3" style="margin-bottom: 5px">
          <label for="SubmissionTypes_${index}__TimeLimit">Time Limit</label>
        </div>
        <div class="editor-field col-xs-1">
           <input class = "pull-right" id="SubmissionTypes_${index}__TimeLimit" name="SubmissionTypes[${index}].TimeLimit" type = 'number' min=1></input>
        </div>
        <div class="editor-field col-xs-3">
        <label for="SubmissionTypes_${index}__MemoryLimit">Memory Limit</label>
        </div>
        <div class="editor-field col-xs-1">
            <input class = "pull-right" id="SubmissionTypes_${index}__MemoryLimit" name="SubmissionTypes[${index}].MemoryLimit" type = 'number' min=1></input>
        </div>
        <div class="editor-field col-xs-5">
            <span class="glyphicon glyphicon-question-sign text-primary" title="Enter solution skeleton" data-tooltip="true"></span>
            <span class="field-validation-valid" data-valmsg-for="SubmissionTypes[${index}].SolutionSkeleton" data-valmsg-replace="true"></span>
        </div>
        <div class="editor-field col-xs-5">
            <span class="field-validation-valid" data-valmsg-for="SubmissionTypes[${index}].TimeLimit" data-valmsg-replace="true"></span>
        </div>
        <div class="editor-field col-xs-5">
            <span class="field-validation-valid" data-valmsg-for="SubmissionTypes[${index}].MemoryLimit" data-valmsg-replace="true"></span>
        </div>
    </div>`;
    };

    const getElementIndex = (el) => {
        return $(el).attr("id").split("_")[1];
    };

    function isChecked(elements) {
        return elements.is(':checked');
    }

    const insertSkeleton = (index, ev) => {
        $(getSolutionSkeletonElement(index))
            .insertAfter($(ev.currentTarget)
                .parent()
                .parent());
    };

    const removeSkeleton = (index) => {
        $(`#SubmissionTypes_${index}__SolutionSkeleton`).parent().parent().remove();
    };
})();