document.getElementById("addUserBtn").addEventListener("click", function (e) {
    e.preventDefault();
    fetch(window.userUrls.createPartial)
        .then(response => response.text())
        .then(html => {
            document.getElementById("createUserModalBody").innerHTML = html;
            var myModal = new bootstrap.Modal(document.getElementById('createUserModal'));
            myModal.show();

            // 初次綁定
            bindCreateUserForm(myModal);
        });
});


function bindCreateUserForm(myModal) {
    const form = document.getElementById("createUserForm");
    if (!form) return;

    // 移除舊的 submit 事件
    form.onsubmit = async function(event) {
        event.preventDefault();
        const formData = new FormData(form);

        const res = await fetch(window.userUrls.createAjax, {
            method: "POST",
            body: formData
        });

        const contentType = res.headers.get("content-type");
        if (contentType && contentType.indexOf("application/json") !== -1) {
            const data = await res.json();
            if (data.success) {
                myModal.hide();
                // 更新列表
                const html = await (await fetch(window.userUrls.usersTablePartial)).text();
                document.querySelector(".list-group").innerHTML = html;
            } else {
                alert(data.message);
            }
        } else {
            const html = await res.text();
            document.getElementById("createUserModalBody").innerHTML = html;
            bindCreateUserForm(myModal); // 重新綁定
        }
    };
}