﻿var welcomeMessage = Vue.component('welcome-message', {
    props: ['title'],
    template: '<h4>Hi {{ title }} from VueJS</h4>'
})

import App from "../_views/App"

var app = new Vue({
    el: '#app',
    template: '<App/>',
    components: {
        'welcomeMessage': welcomeMessage
        App
    }
});