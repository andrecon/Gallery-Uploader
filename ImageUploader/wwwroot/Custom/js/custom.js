//when document is ready, call loadGallery to initialize our dropdown menu
$(function () {
    loadGalleryIds()
});


//Object Array
var FormObjects = [];       //Will hold stuff like [[Files1,Files2,Files3], [ImageCaption1,ImageCaption2,ImageCaption3]]
FormObjects[0] = [];
FormObjects[1] = [];

//Method to get ist of ID and display them in our drodown menu
function loadGalleryIds()
{
    //Call api to get list of all the ids AJAX
    $.ajax({
        type: 'GET',
        url: '/api/Gallery/',
        dataType: 'json',
        success: function(result)
        {
            loadGalleries(result);
        },
        error: function()
        {
            alert("Could Not Load Galleries");
        }
    });
}

function loadGalleries(result)
{
    //Load gally ID to dropdown menu
    if(result != null)
    {
        for(i in result)
        {

            //Using jQuery
            $("#selectImageGallery").append("<option value=' " + result[i].gelleryID + " '>" + result[i].title + "</option>");
        }
    }
}

//load a slider from the gallery IDs. 
function loadSlider(value)
{
    //Using AJAX [HttpGet("{id}")] so we specify id in the url
    $.ajax({
        type: 'GET',
        url: '/api/Gallery/' + value,
        dataType: 'json',
        success: function(data)
        {
            //empty image slider
            $(".swiper-wrapper").html("");
            //Break Package into key value pairs 
            $.each(data, function(key, value)
            {
                //Send these key value pairs to swiper.js
                //Dynamically create our slides
                $('.swiper-wrapper').append("<div class='swiper-slide'><img width='100%' height='500px' src='" + value.image_Path +"' />" + value.image_Caption + "</div>");
                

            });

            var swiper = new Swiper('.swiper-container', {
                  pagination: {
                    el: '.swiper-pagination',
                    type: 'progressbar',
                  },
                  navigation: {
                    nextEl: '.swiper-button-next',
                    prevEl: '.swiper-button-prev',
                  },
             });
        }
    });
}

function AjaxPost(formdata)
{
    //https://developer.mozilla.org/en-US/docs/Web/API/FormData/FormData
    var form_Data = new FormData(formdata);


    for( var i = 0, file; file = FormObjects[0][i]; i++)
    {
        //Files[0], Files[1] ..... 
        form_Data.append('Files[]', file);

        //Delete the old values, so no two files get submitted
        form_Data.delete('Files');
    }

    for (var j= 0, caption; caption = FormObjects[1][i]; j++)
    {
        form_Data.append('ImageCaption[]', caption);
        form_Data.delete('ImageCaption');
    }

    //Submit data into our database using AJAX so we wont have to reload the page
    //Calls PostFormData() function 
    var ajaxOptions =
    {
        type: "POST",
        url: "/api/Gallery",
        data: form_Data,
        success: function(result)
        {
            alert(result);
            window.location.href = "/Home/Index"
        }
    }

    if($(formdata).attr('enctype') == "multipart/form-data")
    {
        ajaxOptions['contentType'] = false;
        ajaxOptions['processData'] = false;
    }

    $.ajax(ajaxOptions);
    return false;

}

//Function to preview files

function PreviewFiles()
{
    var files = document.querySelector('input[type=file]').files;

    //Read the selected files for upload

    function readAndPreview(file)
    {
        //Making sure file.name matches our extension criterias
        //Using Regular Expression


        if(/\.(jpe?g|png|gif)$/i.test(file.name))
        {

            var reader = new FileReader();
            reader.addEventListener("load", function() {
                var image = new Image(200,200);
                image.title = file.name;
                image.border = 2;
                image.src = this.result;
                addImageRow(image);
                countTableRow();
                FormObjects[0].push(file);
            }, false);

            //Helps us read content of the files
            reader.readAsDataURL(file);
        }
    }

    if(files && files[0])
    {
       [].forEach.call(files, readAndPreview);
    }

    //Clear Input type value
    $('input[type="file"]').val(null);
}


//Method to remove files from our list
function removeFile(item)
{
    //Get the row that was clicked to remove
    var row = $(item).closest('tr');

    if ($("#ImageUploadTable tbody tr").length > 1) {
        FormObjects[0].splice(row.index(), 1);
        FormObjects[1].splice(row.index(), 1);
        row.remove();
        //Change counters value
        countTableRow()
        //console.log(FormObjects[0]);
    }
    else if ($("#ImageUploadTable tbody tr").length == 1) {
        $("#ImageUploadTable tbody").remove();
        FormObjects[0] = [];
        FormObjects[1] = [];
        //change counters value
        countTableRow()
    }

}

//Method to clear Preview 
function clearPreview()
{
    if ($("#ImageUploadTable tbody").length > 0) {
        $("#ImageUploadTable tbody tr").remove();
        $("#imgCount").html("<i class='fa fa-images'></i>" + 0);
    }


}



//Method that counts number of files in the table

function countTableRow()
{
    $("#imgCount").html("<i class='fa fa-images'></i> " + $("#ImageUploadTable tbody tr").length);
}


function addImageRow(image)
{
    //Checking if <tbody> tag exist, if not we should add it
    if($("#ImageUploadTable tbody").length == 0)
    {
        //Create table body type
        $("#ImageUploadTable").append("<tbody></tbody>")
    }

    $("#ImageUploadTable tbody").append(BuildImageTableRow(image));
}


// Method to delete preview row
function delPreviewRow(item)
{
    var filename = $(item).closest('[name="photo[]"]');
    alert(filename);
}

//Method to create a new ro for each Imageselected to upload

function BuildImageTableRow(image)
{
    var newRow = "<tr>" +
                    "<td>" +
                        "<div class=''>" +
                            "<img name='photo[]' style='border:1px solid' width='100' height='100' class='image-tag' src= '" + image.src + "' " + "/ >" +
                        "</div>" +
                    "</td>" +
                    "<td>" +
                        "<div class=''>" +
                            "<input name='ImageCaption[]' class='form-control col-xs-3' value='' placeholder='Enter Image Caption' " + "/>" +
                        "</div>" +
                    "</td>" +
                    "<td>" +
                        "<div class-'btn-group' role='group' aria-label='Perform Actions'>" +
                            "<button type='button' name='Edit' class='btn btn-primary btn-sm' onclick='' " + ">" +
                                "<span>" +
                                    "<i class='fa fa-edit'>" + "</i>" +
                                "</span>" +
                            "</button>" +
                            "<button type='button' name='Delete' class='btn btn-danger btn-sm' onclick='removeFile(this)' " + ">" +
                                "<span>" +
                                    "<i class='fa fa-trash'>" + "</i>" +
                                "</span>" +
                            "</button>" +
                        "</div>" + 
                    "</td>" +
                "</tr>";

    return newRow;
    
}

function deleteGallery()
{

    var id = $("#selectImageGallery").val();

    //Adding a bootstrap model to ask if they want to proceed with the deleteion
    $("#DeleteGalleryModal").modal('show');

    $("#DeleteGalleryModal .modal-title").html("Delete Confirmation");
    $("#DeleteGalleryModal .modal-body").html("Do you want to delete " + "<strong class='text-danger'><span id='toDeleteGL'>" + id + "</span></stong>" + " Gallery?");
}

function confimDeleteGallery()
{
    var idGl = $("#toDeleteGL").text();

    //Handle the deletion
    var ajaxOptions = {};

    ajaxOptions.url = "/api/Gallery/" + idGl;
    ajaxOptions.type = "DELETE";
    ajaxOptions.dataType = "json";
    ajaxOptions.success = function ()
    {
        $("#DeleteGalleryModal").modal('hide');
        alert("Deleted Gallery" + idGl);
    };

    ajaxOptions.error = function()
    {
        alert("Could not delete Gallery");
    };

    $.ajax(ajaxOptions);
}


//Method to edit gallery
function editGallery()
{
    var title = $("#selectImageGallery option:selected" ).text();

    var id = $("#selectImageGallery").val();
    $("#EditGalleryModal").modal('show');
    $("#EditGalleryModal .modal-title").html("Edit Gallery : " + title);
    //$("#EditGalleryModal .modal-title").html("Edit Gallery : " + id);
    //If problems, change id="galleryID"
    $("#EditGalleryModal #galleryId").text(id);


    $.ajax({
        type: 'GET',
        url: '/api/Gallery/' + id,
        dataType: 'json',
        success: function(data)
        {
            $("#GalleryTitleEdit").val(data[0].gallery_Title);

            //If any table body, exist...remove it
            $("#EditGalleryTable tbody").remove();
            $("#EditGalleryTable").append("<tbody></tbody>");


            $.each(data, function(key,value)
            {
                $("#EditGalleryTable tbody").append(BuildEditRow(value));
            });
        }
    });
}


//Dynamically create table rows
function BuildEditRow(value)
{
    var newEditRow = "<tr>" +
                        "<td>" +
                            "<div class=''>" + 
                                "<input name='Image_Id[]' hidden class='form-control col-xs-3' value='" + value.image_Id + "'/>" +
                                "<img name='photo[]' style='border:1px solid' width='100' height='100' class='image-tag' src= '" + value.image_Path + "' " + "/ >" +
                            "</div>" +
                        "</td>" +
                        "<td>" +
                            "<div class=''>" +
                                "<input name='ImageCaption[]' class='form-control col-xs-3' value='" + value.image_Caption + "' placeholder='Enter Image Caption' " + "/>" +
                            "</div>" +
                        "</td>" +
                        "<td>" +
                            "<div class-'btn-group' role='group' aria-label='Perform Actions'>" +
                                "<input type='file' name='File[]' style='display:none' onchange='previewImg(this)'" + "/>" +   
                                "<button type='button' name='Upload' class='btn btn-success btn-sm' onclick='openFileExplorer(this)' " + ">" +
                                    "<span>" +
                                        "<i class='fa fa-upload' aria-hidden='true'>" + "</i>" +
                                    "</span>" +
                                "</button>" +
                            "</div>" + 
                        "</td>" +
                    "</tr>"; 


    return newEditRow;
}


function openFileExplorer(item)
{
    //trigger click event for that file
    $(item).closest("tr").find("input[type='file']").trigger('click');
}


//Select the file reader and display it
function previewImg(input) {
    var parent_element = $(input).closest("tr");
    if(input.files && input.files[0])
    {
        var reader = new FileReader();
        reader.onload = function (e) {
            $(parent_element).find('img').attr('src', e.target.result);
        }
        reader.readAsDataURL(input.files[0]);
    }
}


var GalleryObjects = [];       //Will hold stuff like [[Image1,caption1,id1], [Image2,caption2,id2], ...]
GalleryObjects[0] = [];
GalleryObjects[1] = [];

function AjaxUpdateGallery(formData)
{
    var form_Data = new FormData(formData);


    //getAll  method will return the value array associated with the given key from withing the FormData
    var ids = form_Data.getAll('Image_Id[]');
    var captions = form_Data.getAll('ImageCaption[]');
    

    for (var counter = 0; counter < ids.length; counter++)
    {
        GalleryObjects[0].push(ids[counter]);
        GalleryObjects[1].push(captions[counter]);
    }

    for(var i = 0, imageCaption, imageId; imageCaption = GalleryObjects[1][i], imageId = GalleryObjects[0][i]; i++)
    {
        form_Data.append('imageId[]', imageId);
        form_Data.delete('Image_Id[]');
        form_Data.append('imageCaption[]', imageCaption);
        form_Data.delete('ImageCaption[]');
    }

    var id = $("#EditGalleryModal #galleryId").text();

    console.log(formData);

    var ajaxOptions = {
        type: "PUT",
        url: "/api/Gallery/" + id,
        data: form_Data,
        success: function(result)
        {
            alert("Gallery Updated Sucessfully");
            //window.location.href = "/Home/Index";
        },
        error: function () {
            alert("Could Not Update Gallery");
        } 
    }

    //submitting options, making sure its a multiple data
    if($(formData).attr('enctype') == "multipart/form-data")
    {
        ajaxOptions["contenType"] = false;
        ajaxOptions["processData"] = false;
    }
    


    $.ajax(ajaxOptions);
    
    return false;
}