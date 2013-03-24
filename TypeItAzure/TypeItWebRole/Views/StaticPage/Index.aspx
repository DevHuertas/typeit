<%@ Page Title="" Language="C#" %>
<head>
<title>Bootstrap 101 Template</title>
    <meta name="viewport" content="width=device-width, initial-scale=1.0">
    <!-- Bootstrap -->
    <link href="../html/css/bootstrap.css" rel="stylesheet" />
  </head>
  <body>
    <script src="http://code.jquery.com/jquery.js"></script>
    <script src="../html/js/bootstrap.js"></script>
    <script>

        $.get("http://localhost:19500/Home/getuserdata?UserName=Jenny", function (data) {
        });

        $.get("http://localhost:19500/Home/getstats?UserName=Jenny", function (data) {
        });

        function submitSettings() {
            var args = "highlight=" + $("#highlightGroup .active").data("value");
            args += "&sounds=" + $("#soundsGroup .active").data("value");
            args += "&showWord=" + $("#showWordsGroup .active").data("value");
            args += "&blink=" + $("#blinkLettersGroup .active").data("value");

            $.get("http://localhost:19500/Home/updateuserdata?UserName=Jenny&" + args, function (data) {
                alert('User Updated!');
            });
        }

        function jsonUserDataCallback(data) {
            if (data.BlinkLetter) {
                $("#blinkLetterOn").addClass("active");
                $("#blinkLetterOff").removeClass("active");
            }
            else {
                $("#blinkLetterOff").addClass("active");
                $("#blinkLetterOn").removeClass("active");
            }
            if (data.ShowWord) {
                $("#wordsOn").addClass("active");
                $("#wordsOff").removeClass("active");
            }
            else {
                $("#wordsOff").addClass("active");
                $("#wordsOn").removeClass("active");
            }
            if (data.SoundOn) {
                $("#soundsOn").addClass("active");
                $("#soundsOff").removeClass("active");
            }
            else {
                $("#soundsOff").addClass("active");
                $("#soundsOn").removeClass("active");
            }
            if (data.Highlight) {
                $("#highlightOn").addClass("active");
                $("#highlightOff").removeClass("active");
            }
            else {
                $("#highlightOff").addClass("active");
                $("#highlightOn").removeClass("active");
            }
        }

        function jsonUserHistoryCallback(data) {
        }

    </script>
    <section>
        <h2>User Settings for Jenny</h2>
        <section>
        <div class="btn-group" id="showWordsGroup" data-toggle="buttons-radio">
            <h4>Show Words</h4>
          <button type="button" id="wordsOn" data-value="true" class="btn btn-primary active">On</button>
          <button type="button" id="wordsOff" data-value="false" class="btn btn-primary">Off</button>
         </div>
            </section>
        <br /><section>
        <div class="btn-group" id="blinkLettersGroup" data-toggle="buttons-radio">
          <h4>Blink Letters</h4>
          <button type="button" id="blinkLetterOn" data-value="true"  class="btn btn-primary">On</button>
          <button type="button" id="blinkLetterOff" data-value="false" class="btn btn-primary active">Off</button>
         </div>
        </section>
        <br /><section>
        <div class="btn-group" id="soundsGroup" data-toggle="buttons-radio">
            <h4>Sounds</h4>  
          <button type="button" id="soundsOn"  data-value="true" class="btn btn-primary">On</button>
          <button type="button" id="soundsOff"  data-value="false"  class="btn btn-primary active">Off</button>
         </div>
      </section>
        <br />
        <section>
        <div class="btn-group" id="highlightGroup" data-toggle="buttons-radio">
          <h4>Highlighting</h4>  
          <button type="button" id="highlightOn" data-value="true" class="btn btn-primary">On</button>
          <button type="button" id="highlightOff" data-value="false" class="btn btn-primary active">Off</button>
         </div>
    
            <br />
     </section>
        <button type="button" id="submitSettings" onclick="submitSettings()">Save</button>
        <button type="button" id="refresh" onclick="refreshSettings(updateSettings)">Refresh</button>
    </section>

    <section>
        <h2>User Statistics</h2>
    </section>

    <section>
        <h2>Upload Area</h2>
        <p>Insert an Image Word and Video to create one new stage for your child</p>
        <form method="POST" action="../FileManager/Upload" enctype="multipart/form-data" >
            Image: <input type="file" name="uploadedImage" size="60"> <br/><br/>
            Word: <input type="text" name="Word"/><br/><br/>
            Reward Video: <input type="url" name="reward"/><br/><br/>
            <input type="submit" value="Add Metadata"/>
        </form>
    </section>


</body>
