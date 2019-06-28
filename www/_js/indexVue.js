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

var grpc = new Vue({
    el: '#grpc',
    data: {
        messages: [],
        name: "Joe"
    },
    methods: {
        postToGRPC: function (event) {
            var v = this;
            Vue.http.post("/grpc", this.name).then(function (response) {
                v.messages.push(response.body);
            });
        }
    }
})