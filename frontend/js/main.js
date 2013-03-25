window.requestAnimFrame = (function(){
            return  window.requestAnimationFrame       || 
                    window.webkitRequestAnimationFrame || 
                    window.mozRequestAnimationFrame    || 
                    window.oRequestAnimationFrame      || 
                    window.msRequestAnimationFrame     || 
                    function( callback ){
                        window.setTimeout(callback, 1000 / 60);
                    };
})();

//TODO: REFACTOR
// event.type must be keypress
function getChar(event) {

  if (event.which == null) {

    return String.fromCharCode(event.keyCode).toUpperCase(); // IE

  } else if (event.which!=0 && event.charCode!=0) {

    return String.fromCharCode(event.which).toUpperCase();   // the rest

  } else {

    return null // special key

  }

}

//TODO: REFACTOR
jsonCallback = function(json){
    console.log('Got json from image server!');
    
    if(json && json['Src']!==undefined && json['Word']!==undefined)
        TYPEIT.process(json['Src'],json['Word']);
    
}

categoryChange = function(json){
    TYPEIT.onCategory(json);
}

getSodiio = function(json){
    console.log("GOT SODIO:"+json);
    getStory(json,function(canvas,word){
        //convert canvas to img
        //var csrc = canvas.toDataURL();
        TYPEIT.toStory(canvas,word);
    });
}

//RENDER THE SODIIO STORY
function getStory(data,callback) {
    //$.get("http://dialerservice.cloudapp.net:8080/Test/Sodiio", function (data) {
        var stage = document.getElementById('stage');
        var dynamicObject = data;
        //grab our canvas
        var c = document.createElement("canvas");
        var word = "none";
        c.width = stage.width;
        c.height = stage.height;
        //createElement
        var ctx = c.getContext("2d");

        //scle it
        
        ctx.scale(stage.width/1280,stage.height/800);


        //Lets grab page one from the object
        var activePage = dynamicObject.pages[0];
        for (var index in activePage) {
            var item = activePage[index];
            if (index == 0) {
                //image is background so lets do some callibration
                var widthFactor = ctx.canvas.width / item.width;
                var heightFactor = ctx.canvas.height / item.height;
            }

            if (item.type == "image") {
                //We need to get the image and insert into the canvas taking into account the 
                //Height differentials
                //Set the correct dimensions into $img
                var $img = $('<img>', { src: item.url });
                $img[0].container = item;
                //update with the proper position and what not
                $img.load(function() {
                    ctx.drawImage(this, this.container.x, this.container.y, this.container.width, this.container.height);
                });

            } else if (item.type == "text") {
                //Update the active word ?
                word = item.text;
            }
        }
        
        //draw text
        callback(c, word);
    //});
}



var TYPEIT = TYPEIT || (function(){
    
    var first = true;
    
    var dialerserviceurl = "http://dialerservice.cloudapp.net:8080/Home/";
    var dialeractions = {"panic":"Panic","bored":"Bored"};
    
    var imageServiceurl="http://typeit.cloudapp.net/home/";
    
    var categories = ['dog','boat','strawberry','bicycle']
    var catindex = 0;
    
    var rewardlinks = []
    var rewardindex = 0;
    
    var defaults = {"car":""}
    
    var stage = null;
    var twodctx = null;
    
    var currentimg = null;
    var theImg = null;
    
    var currword=null;
    
    var loading=false;
    var waitingimg=false;
    
    var frameCount = 0;
    var circlecount = 0;
    
    //pulse variables
    var pulseFrame = 0;
    var decrease = true;
    var opa = 1.0;
    var doPulse=false;
    
    var wordholder = $('#wordholder');
    //number of letter holders
    var numholders = 10;
    
    var storyMode = false;
    
    function playLoading(){
        var r = 10;
        var i = 1;
        var ccount = 10;
        var offset = ccount*4*r;
        
        offset=offset>stage.width()?0:stage.width()-offset;
        
        frameCount = frameCount+1;
        twodctx.clearRect( 0, 0, stage.width(),stage.height());
        
        if(frameCount%10===0)
            circlecount += 1;
        if(circlecount===ccount)
            circlecount = 0;
            
        while(i<ccount){
            
            if(circlecount===i)
                twodctx.fillStyle='white';
            else
                twodctx.fillStyle='grey';
                
            twodctx.beginPath();
            twodctx.arc(offset/2+i*4*r,stage.height()/2,r,0,2*Math.PI,false);
            twodctx.fill();
            i = i + 1;
        }
        
        if(waitingimg)
            requestAnimFrame(playLoading);
        else{
            frameCount = 0;
            circlecount = 0;
            drawCurrentImg();
        }
    }
    
    
    function setupCanvas()
    {
        twodctx = document.getElementById('stage').getContext('2d');
        resizeCanvas();
        //TODO: do not hard code
        //loadImg(firstimgsrc);
    }
    
    function resizeCanvas()
    {
        sizeCanvas();
        positionCanvas();        
    }
    
    function sizeCanvas()
    {
        var sw = stage.parent().height() * 16.0 / 9.0;
        stage.attr('width',sw);
        stage.attr('height',stage.parent().height());        
    }

    function positionCanvas()
    {
        var sw = stage.parent().height() * 16.0 / 9.0;
        var l =  (stage.parent().width() - sw)/2;
        stage.css('position','absolute');
        stage.css('left',l);
        stage.css('top',0);        
    }


    function loadDefault()
    {
        
    }
    
    function process(imgSrc,newword)
    {
        
        currentimg = imgSrc;
        loadCurrentImg();
        //TODO: figure out why there is a lag
        setTimeout(function(){processWord(newword);},1000);
    }

    function processWord(newword)
    {
        currword = buildWord(newword)
        currword.show();
        currword.showLetterBoxes();
        enableKeyboard();
        //TODO: do only if configured to do so
        doPulse=true;
        pulseCurrentLetter();
    }
    

    function pulseCurrentLetter()
    {
        if(!currword)
            return;
        
        if(!doPulse){
            pulseFrame=0;
            return;
        }
           
        
        var currentLetter = currword.currentLetter();
            
        if(pulseFrame%5==0){
            
            if(opa<=0){
                 decrease = false;
            }
            
            if(opa>1.0){
                 decrease = true;
            }
         
            if(decrease){
                 opa -= 0.1;
            }else{
                 opa += 0.1;
            }
            
            currentLetter.css("opacity",opa);
        }
           
        pulseFrame=pulseFrame+1;   
        window.requestAnimFrame(pulseCurrentLetter);
                
    }

    function enableKeyboard(){
        $('.key').click(function(){
            if(this.id===currword.currentLetter().attr('id')){
                correctLetterSelected();
            }
        });
    }
    
    function disableKeyboard(){
         $('.key').attr('onclick','').unbind('click');      
    }
    
    function correctLetterSelected(){
        doPulse=false;
        currword.showCurrentBox();
        currword.resetLetter();
        //TODO: refactor
        setTimeout(function(){pulseNextLetter();},300);
    }
    
    function processKeyPress(evt){
        var node = (evt.target) ? evt.target : ((evt.srcElement) ? evt.srcElement : null);
        var charPressed = getChar(evt);
        console.log(charPressed);
        if(charPressed===currword.currentChar()){
            correctLetterSelected();
        }else{
            //TODO: gather stats
        }     
            
    }
        
    function reset(){
        //currword.completed();
        //currword.timer = null;
        //currword = null;
        //CAREFUL
        waitingimg=false;
        catindex = catindex+1;
        pulseFrame = 0;
        decrease = true;
        opa = 1.0;
        disableKeyboard();
        doPulse=false;
        fetchNextImg();
    }

    function pulseNextLetter(){
        if(currword.moreLetters()){
            currword.toNextLetter();
            doPulse=true;
            pulseCurrentLetter();
        }
        else{
            currword.completed();
            reward();
        }
    }

    function buildTypeTimer(fromWord){
        var timer= {
            correctcount:0,
            incorrectcount:0,
            word:fromWord,
            lastkeyhit:0,
            bored:false,
            panic:false,
            wordtime:0,
            startedAt: null,
            start: function(){
                startedAt = new Date().getMilliseconds();
                window.requestAnimFrame(timer.tick());
            },
            tick: function(){
                console.log('tick');
            },
            stop: function(){
                this.wordtime = (new Date().getMilliseconds()-this.startedAt);
                console.log('word was complete after ['+this.wordtime+'] ms!');
            },
            send : function(){
                console.log("bored:"+this.bored);
                console.log("panic:"+this.panic);
                //send stats
                var url = "http://typeit.cloudapp.net/home/stats?UserName=Jenny&Word="+this.word+"&Duration="+this.wordtime+"&Panic="+this.panic+"&Bored="+this.bored;
               $.ajax({
                     type: 'GET',
                     url: url,
                     async: true,
                     jsonpCallback: '',
                     contentType: "application/json",
                     dataType: 'jsonp',
                     success: function(json) {
                        console.dir(json.sites);
                     },
                     error: function(e) {
                        console.log(e.message);
                     }
            });
            }
        }
        
        return timer;
    }

    function buildWord(newword)
    {
        
            
        return {
            word:newword.toUpperCase(),
            length:newword.length,
            windex:0,
            lindex:0,
            done:false,
            timer: buildTypeTimer(newword),
            show:function(){
                this.highlight();
            },
            highlight:function(){
                var c=0;
                while(c<this.length){
                    var selector = "#"+this.word.charAt(c);
                    $(selector).css('background-color','#fff203');
                    c=c+1;
                }
            },
            resetLetters:function(){
                var c=0;
                while(c<this.length){
                    var selector = "#"+this.word.charAt(c);
                    $(selector).css('opacity',1.0);
                    $(selector).css('background-color','#c4d739');
                    c=c+1;
                }
            },
            currentLetter:function(){
                if(this.windex>this.length)
                    return null;
                var selector = "#"+this.word.charAt(this.windex);
                return $(selector);
            },
            currentChar:function(){
               return this.word.charAt(this.windex); 
            },
            charAt:function(index){
                return this.word.charAt(index);
            },
            moreLetters:function(){
                return this.windex+1<this.length;
            },
            toNextLetter:function(){
                this.windex=this.windex+1;
            },
            resetLetter:function(){
                this.currentLetter().css('opacity',1.0);
                this.currentLetter().css('background-color','#c4d739');
            },
            showLetterBoxes:function(){
                
                //TODO: refactor
                this.lindex =Math.ceil((numholders-this.length)/2);
                for(var i=0;i<this.length;i++){
                    var selector = '#l'+(this.lindex+i+1);
                    $(selector).fadeTo('slow',1.0);
                    $(selector).append(currword.charAt(i));
                }                
            },
            showCurrentBox: function(){
                var selector = '#l'+(this.lindex+this.windex+1);
                $(selector).css('color','#fff203');
            },
            clearBoxes: function(){
                for(var i=0;i<this.length;i++){
                    var selector = '#l'+(this.lindex+i+1);
                    $(selector).css('color','#007dc3');
                    $(selector).fadeTo('fast',0.1);
                    $(selector).html("");
                } 
            },
            completed: function(){
                this.done = true;
                this.clearBoxes();
                //stop gathering stats
                this.timer.stop();
                //send stats
                try{
                    this.timer.send();
                }catch(err){
                    console.log('oops');
                }

            }
        } 
    }

    function drawCurrentImg(){
        if(!theImg)
            return;
        twodctx.drawImage(theImg,0,0,stage.width(),stage.height());
    }
    
    function loadCurrentImg()
    {
        console.log('Drawing: '+currentimg);
        twodctx.clearRect( 0, 0, stage.width(),stage.height());
        theImg = new Image();
        theImg.src = currentimg;
        theImg.onload = function(e){
            twodctx.globalAlpha = 1.0;
            waitingimg=false;
            //TODO: refactor
            console.log('done drawing image!');
        };

    }

    function getPayload()
    {
        //TODO: eventually we wont need a global function
        (function($) {
            var url = 'http://typeit.cloudapp.net/home/index?category='+categories[catindex];

            $.ajax({
               type: 'GET',
                url: url,
                async: true,
                jsonpCallback: '',
                contentType: "application/json",
                dataType: 'jsonp',
                success: function(json) {
                   console.dir(json.sites);
                },
                error: function(e) {
                   console.log(e.message);
                }
            });
        
        })(jQuery);
    }

    function panic()
    {
        console.log('panic!');
        showModal('panic','http://www.youtube.com/embed/3ichQOqbewA?autoplay=1&cc_load_policy=1');
        //tell timer that a panic event occurred
        currword.timer.panic = true;
        
        //TODO: eventually we wont need a global function
        
        (function($) {
            var url = 'http://dialerservice.cloudapp.net:8080/Home/Panic';

            $.ajax({
               type: 'GET',
                url: url,
                async: true,
                jsonpCallback: '',
                contentType: "application/json",
                dataType: 'jsonp',
                success: function(json) {
                   console.dir(json.sites);
                },
                error: function(e) {
                   console.log(e.message);
                }
            });
        
        })(jQuery);
        
    }
    
    function bored()
    {
        console.log('bored!');
        showModal('bored','http://www.youtube.com/embed/3ichQOqbewA?autoplay=1&cc_load_policy=1');
        dialer.changeCategory();
        currword.timer.bored = true; 
        //fetchSodiio();
    }
    
    function reward()
    {
        console.log('reward!');
        showModal('reward','http://www.youtube.com/embed/5NqMWsQCUQE?autoplay=1&cc_load_policy=1',reset);
        //TODO: 
        
    }

    function showModal(modalid,youtubeurl,callback){
        var selector = "#"+modalid;
        var bodyselector = selector + " .modal-body";
        $(bodyselector).append("<iframe width='425' height='239' src='"+youtubeurl+"' frameborder='0'allowfullscreen></iframe>");
        $(selector).on('hide',function(){$(bodyselector).html("");if(callback){callback();}});
        $(selector).modal('show');        
    }

    function fetchNextImg(){

        if(storyMode){
            processWord("school"); //HACK!!!
        }
        else if(first || currword.done){
            first=false;
            waitingimg=true;
            requestAnimFrame(playLoading);
            getPayload();
        }
    }
    
    
    dialer = {
        url:dialerserviceurl,
        category:"",
        polling:true,
        pollcount:0,
        operations:{"clean":"Clean","category":"RequestForNewContent/1","change":"RequestNewContent"},
        clean: function(){
            this.doOperation("clean");
            this.fetchCategory();
        },
        fetchCategory: function(){
            this.doOperation("category");
        },
        changeCategory: function(){
            this.doOperation("change");
            this.polling = true;
            window.requestAnimFrame(dialer.pollForCategory);
            this.pollForCategory();
        },
        pollForCategory: function(t){
            if(dialer.polling)
            {
                if(dialer.pollcount%60===0)
                    dialer.fetchCategory();
                dialer.pollcount = dialer.pollcount+1;
                window.requestAnimFrame(dialer.pollForCategory);
            }else{
                dialer.pollcount = 0;
            }
        },
        doOperation:function(operation){
            var that = this;
            (function($) {
                var url = that.url+that.operations[operation];

                $.ajax({
                    type: 'GET',
                    url: url,
                    async: true,
                    jsonpCallback: '',
                    contentType: "text/plain",
                    dataType: 'jsonp',
                    success: function(json) {
                        console.dir(json.sites);
                    },
                    error: function(e) {
                        console.log(e.message);
                    }
                });
        
            })(jQuery);
        }
    }
    
    function fetchSodiio(){
        (function($) {
            var url = 'http://dialerservice.cloudapp.net:8080/Test/Sodiio';

            $.ajax({
               type: 'GET',
                url: url,
                async: true,
                jsonpCallback: '',
                contentType: "application/json",
                dataType: 'jsonp',
                success: function(json) {
                   console.dir(json.sites);
                },
                error: function(e) {
                   console.log(e.message);
                }
            });
        
        })(jQuery);
    }
    
    return {
        init: function(){
            console.log('init');
            stage = $('#stage');
            //reset dialer
            dialer.clean();
            $(window).keypress(function(evt){processKeyPress(evt);});
            setupCanvas();
            $(window).resize(function(e){console.log('resize');resizeCanvas();drawCurrentImg();});
            fetchNextImg();
            $('.panicholder').click(function(){panic();});
            $('.frownholder').click(function(){bored();});
        },
        getStage: function(){return stage;},
        process: process,
        loadNextDefaultImage: loadDefault,
        theword:function(){return currword;},
        onCategory: function(category){
            console.log("Category is:"+category);
            if(!dialer.category)
                dialer.category = category;
            if(dialer.category && dialer.category !== category){
                console.log("GOT A NEW CATEGORY");
                dialer.polling = false;
                fetchSodiio();
            }
        },
        toStory: function(canvas,word){
            var left = stage.css('left');
            var top = stage.css('top');
            //var context = 
            canvas.id ="story";
            stage.replaceWith(canvas);
            
            //$(document.body).append("<div style='z-index:5000;position:absolute;top:"+(parseInt(top,10)-15)+";left:"+(parseInt(left,10)-10)+"'>I go to </div>");
            
            $('.sodiio').css('top',top);
            $('.sodiio').css('left',left);
            $('.sodiio').show();
            
            $("#story").css('position','absolute');
            $("#story").css('top',top);
            $("#story").css('left',left);
            storyMode = true;
            doPulse=false;
            waitingimg = true;
            setTimeout(function(){
                    $('#bored').modal('hide');
                    currword.resetLetters();
                    currword.completed();
                    reset();
            },300);

        }
    }
}());