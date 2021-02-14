$(document).ready(function () {
    $("[data-action]").on("click", function (e) {
        e.preventDefault();

        $("#EventCommand").val($(this).data("action"));
        $("#EventArgument").val($(this).attr("data-val"));

        var deletelabel = '';
        var restoreLabel = '';
        var submit = true;

        if ($(this).data("action") === "delete") {
            deletelabel = $(this).data("deletelabel");
            if (!deletelabel) {
                deletelabel = 'Record';
            }
            if (!confirm("Are you sure you want to delete this " + deletelabel + " ?")) {
                submit = false;
            }
        }
        else if ($(this).data("action") === "restore") {
            restoreLabel = $(this).data("restoreLabel");
            if (!restoreLabel) {
                restoreLabel = 'Record';
            }
            if (!confirm("Are you sure you want to delete? Your added " + restoreLabel + "will be permanently deleted. This action cannot be undone.")) {
                submit = false;
            }
            else {
                $("#searchField").val("")
            }
        }
        if (submit) {
            $("form").submit();
        }
    });

    $("#btnReset").click(function () {
        $("#searchField").val("")
    });
    
});
