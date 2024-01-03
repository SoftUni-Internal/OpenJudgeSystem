(() => {
    $(document).ready(function () {
        const submissionTypesCheckboxes = $("input[name^='SubmissionTypes'][name$='.IsChecked']");

        submissionTypesCheckboxes.change((ev) => {
            try {
                var submissionTypeId = ev.target.parentNode.parentNode.children[0].children[0].children[0].value;
                var submissionTypeName = ev.target.parentNode.parentNode.children[0].children[0].children[1].value;
                const index = getElementIndex(ev.currentTarget);
                if (isChecked($(ev.currentTarget))) {
                    insertSkeleton(index, ev);
                    updateKendoDropdown(true,submissionTypeId, submissionTypeName);
                } else {
                    removeSkeleton(index);
                    updateKendoDropdown(false,submissionTypeId, submissionTypeName);
                }
            } catch (ex) {
                console.log(ex);
            }
        });
    });

    const getSolutionSkeletonElement = (index) => {
        return `<div class="row" style="margin-bottom: 2rem">
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
         <div class="editor-field col-xs-3">
            <label for="SubmissionTypes_${index}__WorkerType">Worker Type</label>
        </div>
        <div class="editor-field col-xs-1">
            <select id="SubmissionTypes_${index}__WorkerType" class="pull-right" name="SubmissionTypes[${index}].WorkerType"></select>
        </div>
        <div class="editor-field col-xs-5">
            <span class="glyphicon glyphicon-question-sign text-primary" title="Enter solution details" data-tooltip="true"></span>
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

    const updateKendoDropdown = (isChecked, id, name) => {
        let dropdown = $("#DefaultSubmissionTypeId").data("kendoDropDownList");
        let dataSource = dropdown.dataSource;
        if(isChecked){
            let option = new Object();
            option.Text = name;
            option.Value = id;
            dataSource.add(option);
        }else {
            let itemToRemove =dataSource.data().find(item => item.Value === id);
            dataSource.remove(itemToRemove);
        }
        dropdown.refresh();
    }
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
        $("#SubmissionTypes_" + index + "__WorkerType").kendoDropDownList({
            dataTextField: "Text",
            dataValueField: "Value",
            dataSource: window.workerTypesData
        });
    };

    const removeSkeleton = (index) => {
        $(`#SubmissionTypes_${index}__SolutionSkeleton`).parent().parent().remove();
    };
})();

