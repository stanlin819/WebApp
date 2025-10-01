$(document).ready(() =>{
    const userId = window.videoConfig.userId;
    $("#todoForm").on("submit", (e) =>{
        e.preventDefault();
        const todoText = $("#todoInput").val().trim();

        if(todoText == ""){
            $("#errorMsg").text("Please enter your to-do items!");
            return;
        }
        $("#errorMsg").text("");

        // $.ajax({
        //     url: "/api/TodoAPI/AddTodo",
        //     type: "POST",
        //     contentType: "application/json",
        //     data: JSON.stringify({ text: todoText, userId: userId }),
        //     dataType: "json",
        //     success: function(todo) {
        //         renderTodo(todo);
        //         $("#todoInput").val("");
        //     },
        //     error: function(xhr, status, error) { // 失敗時執行
        //         console.error("An error occurred:", error);
        //     },
        // });

        $.post("/api/TodoAPI/AddTodo", { text: todoText, userId: userId }, function(todo){
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
            }.bind(this),
            error: function(xhr, status, error) {
                console.error("An error occurred:", error);
            }
        });
    });

    li.attr("data-id", todo.id);
    if(todo.done) li.addClass("done");
    li.append(todo.text);
    li.append(deleteBtn);
    $("#todoList").append(li);

}