$(document).ready(() =>{
    $.ajaxSetup({
        contentType: "application/json",
    });

    $(document).ajaxStart(function() {
        $("#loadingSpinner").show();
    });

    $(document).ajaxStop(function() {
        $("#loadingSpinner").hide();
    });

    const userId = window.videoConfig.userId;
    $("#todoForm").on("submit", (e) =>{
        e.preventDefault();
        const todoText = $("#todoInput").val().trim();

        if(todoText == ""){
            $("#errorMsg").text("Please enter your to-do items!");
            return;
        }
        $("#errorMsg").text("");

        $.post("/api/TodoAPI/AddTodo", JSON.stringify({ text: todoText, userId: userId }), function(todo){
            renderTodo(todo);
            $("#todoInput").val("");
        });
    });

    $("#todoList").on("click", "li", function(){
        // $.get("/TodoList/Toggle", {todoId : $(this).attr("data-id")}, (res) =>{
        //     if(res.success)
        //         $(this).toggleClass("done");
        // })

        $.ajax({
            url: "/api/TodoAPI/Toggle/" + $(this).data("id"),
            type: "PATCH", // 指定 PATCH
            success: function() {
                $(this).toggleClass("done");
            }.bind(this),
            error: function(xhr, status, error) {
                console.error("An error occurred:", error);
            }
        });
    });

    $.get("/api/TodoAPI/GetTodos", {userId: userId}, function(data){
        if(data){
            data.forEach(element => {
                renderTodo(element);
        });
        }
    })
});

function renderTodo(todo){
    let li = $("<li>");
    let deleteBtn = $("<a>").attr("style", "display: inline; color: red;");
    deleteBtn.addClass("bi bi-trash");

    deleteBtn.click(function(e){
        e.stopPropagation()
        // $.post("/TodoList/Delete", { todoId: todo.id }, function(res){
        //     if(res.success)
        //         li.remove();
        //     else
        //         alert(res.message);
        // });
        $.ajax({
            url: "/api/TodoAPI/DeleteTodo/" + todo.id,
            type: "DELETE",
            success: function() {
                li.remove();
            },
            error: function(xhr, status, error) {
                console.error("An error occurred:", error);
            }
        });
    });

    li.attr("data-id", todo.id);
    if(todo.done) li.addClass("done");
    li.append(todo.text);
    li.append(deleteBtn);
    $("ul[id = 'todoList']").append(li);
}