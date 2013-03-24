<%@ Page Title="" Language="C#" %>
<head>
<title>Bootstrap 101 Template</title>
    <meta name="viewport" content="width=device-width, initial-scale=1.0">
    <!-- Bootstrap -->
    <link href="../html/css/bootstrap.css" rel="stylesheet" />
      <style type="text/css">
#grid {
    display: -ms-grid;
    -ms-grid-columns: auto 1fr;
    -ms-grid-rows: auto 1fr auto;
}
 
</style>


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
            document.getElementById('loading').style.display = "none";

            //this will return a list
            $.each(data, function (key, value) {

                var oRow = document.getElementById('historyTable').insertRow(-1);

                var oCell = oRow.insertCell(-1);
                oCell.innerHTML = value.WordText;
                
                oCell = oRow.insertCell(-1);
                oCell.innerHTML = value.Bored;
                oCell.style.backgroundColor = "lightblue";

                oCell = oRow.insertCell(-1);
                oCell.innerHTML = value.Panic;

                oCell = oRow.insertCell(-1);
                oCell.innerHTML = value.Duration;
                oCell.style.backgroundColor = "lightblue";

                oCell = oRow.insertCell(-1);
                if (value.ImageUrl != null && value.ImageUrl != "") {
                    oCell.innerHTML = '<a target="_blank" href="' + value.ImageUrl + '">Image Link</a>';
                }
                else {
                    oCell.innerHTML = "None";
                }

                oCell = oRow.insertCell(-1);
                oCell.style.backgroundColor = "lightblue";
                if (value.RewardUrl != null && value.RewardUrl != "") {
                    oCell.innerHTML = '<a target="_blank" href="' + value.RewardUrl + '">Video Link</a>';
                }
                else {
                    oCell.innerHTML = "None";
                }
            });
        }

    </script>
      <h1>Jenny's TypeIt!</h1>
      <hr />
        <section style="width:640px;margin-left:50px">
        <h2>Settings</h2>
        <div class="btn-group" id="showWordsGroup" data-toggle="buttons-radio">
            <h4>Show Words</h4>
          <button type="button" id="wordsOn" data-value="true" class="btn btn-primary active">On</button>
          <button type="button" id="wordsOff" data-value="false" class="btn btn-primary">Off</button>
         </div>
        <br />
        <div class="btn-group" id="blinkLettersGroup" data-toggle="buttons-radio">
          <h4>Blink Letters</h4>
          <button type="button" id="blinkLetterOn" data-value="true"  class="btn btn-primary">On</button>
          <button type="button" id="blinkLetterOff" data-value="false" class="btn btn-primary active">Off</button>
         </div>
        <br />
        <div class="btn-group" id="soundsGroup" data-toggle="buttons-radio">
            <h4>Sounds</h4>  
          <button type="button" id="soundsOn"  data-value="true" class="btn btn-primary">On</button>
          <button type="button" id="soundsOff"  data-value="false"  class="btn btn-primary active">Off</button>
         </div>
        <br />
        <div class="btn-group" id="highlightGroup" data-toggle="buttons-radio">
          <h4>Highlighting</h4>  
          <button type="button" id="highlightOn" data-value="true" class="btn btn-primary">On</button>
          <button type="button" id="highlightOff" data-value="false" class="btn btn-primary active">Off</button>
         </div>
    
                 </br>
        <button type="button" style="width:200px;margin-left:auto;margin-right:auto;" id="submitSettings" onclick="submitSettings()">Save Settings</button>
     </section>
    <br />

    <section>
        <h2>User Statistics</h2>
        <h3 id="loading" style="color:red">Loading....</h3>
        <table id="historyTable" style="border: 2px solid black;">
            <thead><tr>
            <th >Word</th>
            <th style="background-color:lightblue" >Was Bored</th>
            <th >Panicked!</th>
            <th style="background-color:lightblue">Duration</th>
<th>Image Link</th>
<th style="background-color:lightblue">Reward Link</th>
            </tr>
             </thead>
       </table>
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
