
$(document).ready(function () {
    $("#perishableSelect").change(function () {
        if ($(this).val() === "True") {
            $(".perishable-fields").show();
        } else {
            $(".perishable-fields").hide();
        }
    });

    // Inicialmente, esconda o campo se a opção for "Não"
    if ($("#perishableSelect").val() === "False") {
        $(".perishable-fields").hide();
    }
});