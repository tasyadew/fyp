using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Net.Http;
using TMPro;
using System.Text;
using UnityEngine.SceneManagement;

public class APIManager : MonoBehaviour
{
    public class APIObject
    {
        public string title { get; set; }
    }

    [SerializeField] List<ProductObject> allProducts;

    string url = "http://app.tasyadew.com:8500/predict";
    TextMeshProUGUI errorMessage;
    TMP_InputField inputProduct;
    GameObject productList;
    GameObject template;

    void Start()
    {
        // Get the components of TextMeshPro InputField and Text objects
        errorMessage = GameObject.Find("ErrorMessage").GetComponent<TextMeshProUGUI>();
        inputProduct = GameObject.Find("InputProduct").GetComponent<TMP_InputField>();

        // Get the parent object of the product list and the template object
        productList = GameObject.Find("ProductList");
        template = productList.transform.GetChild(0).gameObject;

        RerenderList();
    }

    public async void OnAddItem()
    {
        if (inputProduct.text != "")
        {
            var productName = "N/A";
            errorMessage.text = "Loading...";
            using (var httpClient = new HttpClient())
            {
                var json = "{\"title\":\"" + inputProduct.text + "\"}";
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var response = await httpClient.PostAsync(url, content);
                if (response.IsSuccessStatusCode)
                {
                    errorMessage.text = "";
                    // Currently, the response body is a string, so we want to convert it into an array and retrieve the
                    productName = await response.Content.ReadAsStringAsync();

                    if (productName == "Unknown Category")
                    {
                        errorMessage.text = "Error: Unknown Category";
                        productName = "N/A";
                        Debug.Log("Error: " + inputProduct.text + " is an unknown category");
                        return;
                    }

                    // Add the product to the list
                    productName = productName.Replace("\"", "");
                    allProducts.Add(new ProductObject { name = inputProduct.text, category = productName });
                    UpdateList();
                }
                else
                {
                    // Display the error message from response body
                    string tempText = await response.Content.ReadAsStringAsync();
                    errorMessage.text = response.ReasonPhrase;
                    Debug.Log("Error: " + response.ReasonPhrase);
                    Debug.Log(tempText);
                }
            }
        }
    }

    public void OnDeleteItem(int position)
    {
        Debug.Log("Delete button clicked at position " + position);
        allProducts.RemoveAt(position);
        RerenderList();

        Debug.Log(allProducts);
    }

    void UpdateList()
    {
        GameObject tempObject = Instantiate(template, productList.transform);
        tempObject.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = allProducts[allProducts.Count - 1].name;
        tempObject.transform.GetChild(1).GetComponent<TextMeshProUGUI>().text = allProducts[allProducts.Count - 1].category;
        int pos = allProducts.Count - 1;
        tempObject.transform.GetChild(2).GetComponent<Button>().onClick.AddListener(() => OnDeleteItem(pos));
        tempObject.SetActive(true);
    }

    void RerenderList()
    {
        // Delete all the children other than the template
        for (int i = productList.transform.childCount - 1; i > 0; i--)
        {
            Destroy(productList.transform.GetChild(i).gameObject);
        }

        // Set the template to active and instantiate the child objects based on allProducts
        GameObject tempObject;
        for (int i = 0; i < allProducts.Count; i++)
        {
            // Instantiate the child object and set the parent to "ProductList"
            tempObject = Instantiate(template, productList.transform);
            tempObject.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = allProducts[i].name;
            tempObject.transform.GetChild(1).GetComponent<TextMeshProUGUI>().text = allProducts[i].category;
            int pos = i;
            tempObject.transform.GetChild(2).GetComponent<Button>().onClick.AddListener(() => OnDeleteItem(pos));
            tempObject.SetActive(true);
        }
    }

    public void OnSubmitList()
    {
        if (allProducts.Count == 0)
        {
            errorMessage.text = "Error: No products added";
            return;
        }
        ProductsData.list = allProducts;
        SceneManager.LoadScene("Main");
    }
}