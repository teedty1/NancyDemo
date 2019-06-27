var sqs = new Vue({
    el: '#sqs',
    data: {
        messages: []
    },
    methods: {
        postToSQS: function (event) {
            var v = this;
            Vue.http.post("/sqs").then(function (response) {
                v.messages.push(response.body);
            });
        }
    }
})