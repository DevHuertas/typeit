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
    
    if(!json)
       TYPEIT.loadNextDefaultImage();
    else
        TYPEIT.process(json['Src'],json['Word']);
    
}

var TYPEIT = TYPEIT || (function(){
    
    var dialerserviceurl = "http://dialerservice.cloudapp.net:8080/Home/";
    var dialeractions = {"panic":"Panic","bored":"Bored"};
    
    var imageServiceurl="http://typeit.cloudapp.net/home/";
    
    var categories = ['boat','strawberry','Dog']
    var catindex = 0;
    
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
        currword.clearBoxes();
        currword = null;
        //CAREFUL
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
        else
            reward();
    }

    function buildWord(newword)
    {
        return {
            word:newword.toUpperCase(),
            length:newword.length,
            windex:0,
            lindex:0,
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
        

        
        //TODO: eventually we wont need a global function
        /*
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
        */
    }
    
    function bored()
    {
        console.log('bored!');
        showModal('bored','http://www.youtube.com/embed/3ichQOqbewA?autoplay=1&cc_load_policy=1');
        

        
        //TODO: 
        
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
        waitingimg=true;
        requestAnimFrame(playLoading);
        getPayload();
    }
    
    return {
        init: function(){
            console.log('init');
            stage = $('#stage');
            $(window).keypress(function(evt){processKeyPress(evt);});
            setupCanvas();
            $(window).resize(function(e){console.log('resize');resizeCanvas();drawCurrentImg();});
            fetchNextImg();
            $('.panicholder').click(function(){panic();});
        },
        getStage: function(){return stage;},
        process: process,
        loadNextDefaultImage: loadDefault,
        theword:function(){return currword;}
    }
}());