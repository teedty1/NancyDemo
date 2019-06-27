var welcomeMessage = Vue.component('welcome-message', {
    props: ['title'],
    template: '<h4>Hi {{ title }} from VueJS</h4>'
})

var app = new Vue({
    el: '#app',
    components: {
        'welcomeMessage': welcomeMessage
    }
});