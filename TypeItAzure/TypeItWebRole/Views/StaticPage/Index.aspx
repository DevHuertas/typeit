﻿<%@ Page Title="" Language="C#" %>
<head>
<title>Bootstrap 101 Template</title>
    <meta name="viewport" content="width=device-width, initial-scale=1.0">
    <!-- Bootstrap -->
    <link href="css/bootstrap.css" rel="stylesheet" />
  </head>
  <body>
    <script src="http://code.jquery.com/jquery.js"></script>
    <script src="js/bootstrap.js"></script>
    <script>
        function submitSettings() {
            alert($("#blinkLettersGroup .active").data("value"));
        }

        function jsonCallback(json) {
        }

        function refreshSettings(callback) {
            $.get("http://localhost:19500/Home/getuserdata?UserName=Jenny", function (data) {
            });
        }

    </script>
    <section>
        <h2>User Settings</h2>
        <section>
        <div class="btn-group" id="showWordsGroup" data-toggle-name="is_private" data-toggle="buttons-radio">
            <h4>Show Words</h4>
          <button type="button" id="wordsOn" data-value="1" class="btn btn-primary active">On</button>
          <button type="button" id="wordsOff" data-value="0" class="btn btn-primary">Off</button>
         </div>
            </section>
        <section>
        <div class="btn-group" id="blinkLettersGroup" data-toggle="buttons-radio">
          <h4>Blink Letters</h4>
          <button type="button" id="lettersOn" data-value="1" class="btn btn-primary">On</button>
          <button type="button" id="lettersOff" data-value="0" class="btn btn-primary active">Off</button>
         </div>
        </section>
        <section>
        <div class="btn-group" data-toggle="buttons-radio">
            <h4>Sounds</h4>  
          <button type="button" id="soundsOn" class="btn btn-primary">On</button>
          <button type="button" id="soundsOff" class="btn btn-primary">Off</button>
         </div>
      </section>
        <section>
        <div class="btn-group" data-toggle="buttons-radio">
          <h4>Highlighting</h4>  
          <button type="button" id="highlightOn" class="btn btn-primary">On</button>
          <button type="button" id="highlightOff" class="btn btn-primary">Off</button>
         </div>
    
            
     </section>
        <button type="button" id="submitSettings" onclick="submitSettings()">Save</button>
        <button type="button" id="refresh" onclick="refreshSettings(updateSettings)">Refresh</button>
    </section>

    <section>
        <h2>User Statistics</h2>
    </section>

    <section>
        <h2>Upload Area</h2>
        <div>Image</div>
        <div>Word</div>
        <div>Reward Video</div>
    </section>


</body>
