#include <poppler/cpp/poppler-document.h>
#include <poppler/cpp/poppler-page.h>
#include <iostream>
#include <string>

int main(int argc, char *argv[]) {
    if (argc < 2) {
        std::cerr << "Usage: " << argv[0] << " <pdf-file-path>" << std::endl;
        return 1;
    }

    std::string pdfFilePath = argv[1];

    // Open the PDF document
    poppler::document* doc = poppler::document::load_from_file(pdfFilePath);
    if (!doc) {
        std::cerr << "Error: Unable to open PDF file " << pdfFilePath << std::endl;
        return 1;
    }

    // Iterate through the pages
    for (int i = 0; i < doc->pages(); ++i) {
        // Get a page
        poppler::page* p = doc->create_page(i);
        if (p == nullptr) {
            std::cerr << "Error: Unable to get page " << i << std::endl;
            continue;
        }

        // Extract text from the page
        std::string text = p->text().to_latin1();
        std::cout << "Page " << i + 1 << ":\n" << text << std::endl;

        delete p;
    }

    return 0;
}