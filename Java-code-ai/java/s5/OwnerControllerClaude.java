package java.s5;

import java.util.List;
import java.util.Map;

import javax.validation.Valid;

import org.springframework.data.domain.Page;
import org.springframework.data.domain.PageRequest;
import org.springframework.data.domain.Pageable;
import org.springframework.samples.petclinic.visit.VisitRepository;
import org.springframework.stereotype.Controller;
import org.springframework.ui.Model;
import org.springframework.validation.BindingResult;
import org.springframework.web.bind.WebDataBinder;
import org.springframework.web.bind.annotation.GetMapping;
import org.springframework.web.bind.annotation.InitBinder;
import org.springframework.web.bind.annotation.PathVariable;
import org.springframework.web.bind.annotation.PostMapping;
import org.springframework.web.bind.annotation.RequestParam;
import org.springframework.web.servlet.ModelAndView;

/**
 * Controller for handling Owner-related operations in the pet clinic application.
 * Manages creating, reading, updating owners and displaying their details.
 */
@Controller
class OwnerControllerClaude {

    // Constants
    private static final String VIEWS_OWNER_CREATE_OR_UPDATE_FORM = "owners/createOrUpdateOwnerForm";
    private static final String FIND_OWNERS_VIEW = "owners/findOwners";
    private static final String OWNERS_LIST_VIEW = "owners/ownersList";
    private static final String OWNER_DETAILS_VIEW = "owners/ownerDetails";
    private static final int PAGE_SIZE = 5;

    // Repositories
    private final OwnerRepository ownerRepository;
    private final VisitRepository visitRepository;

    /**
     * Constructor for dependency injection of required repositories.
     */
    public OwnerControllerClaude(OwnerRepository ownerRepository, VisitRepository visitRepository) {
        this.ownerRepository = ownerRepository;
        this.visitRepository = visitRepository;
    }

    /**
     * Prevents direct modification of the id field in form submissions.
     */
    @InitBinder
    public void setAllowedFields(WebDataBinder dataBinder) {
        dataBinder.setDisallowedFields("id");
    }

    /**
     * Displays the form for creating a new owner.
     */
    @GetMapping("/owners/new")
    public String initCreationForm(Map<String, Object> model) {
        model.put("owner", new Owner());
        return VIEWS_OWNER_CREATE_OR_UPDATE_FORM;
    }

    /**
     * Processes the form submission for creating a new owner.
     */
    @PostMapping("/owners/new")
    public String processCreationForm(@Valid Owner owner, BindingResult result) {
        if (result.hasErrors()) {
            return VIEWS_OWNER_CREATE_OR_UPDATE_FORM;
        }
        
        this.ownerRepository.save(owner);
        return redirectToOwnerDetails(owner.getId());
    }

    /**
     * Displays the form for finding owners.
     */
    @GetMapping("/owners/find")
    public String initFindForm(Map<String, Object> model) {
        model.put("owner", new Owner());
        return FIND_OWNERS_VIEW;
    }

    /**
     * Processes the owner search form and handles pagination.
     * Three possible outcomes:
     * 1. No owners found - returns to search form with error
     * 2. One owner found - redirects to that owner's details
     * 3. Multiple owners found - displays paginated list
     */
    @GetMapping("/owners")
    public String processFindForm(@RequestParam(defaultValue = "1") int page, Owner owner, 
                                 BindingResult result, Model model) {
        // Handle empty search (return all owners)
        if (owner.getLastName() == null) {
            owner.setLastName(""); // empty string for broadest search
        }

        // Find owners by last name with pagination
        String lastName = owner.getLastName();
        Page<Owner> ownersResults = findPaginatedByLastName(page, lastName);
        
        if (ownersResults.isEmpty()) {
            // No owners found
            result.rejectValue("lastName", "notFound", "not found");
            return FIND_OWNERS_VIEW;
        } 
        else if (ownersResults.getTotalElements() == 1) {
            // Single owner found - redirect to details
            owner = ownersResults.iterator().next();
            return redirectToOwnerDetails(owner.getId());
        } 
        else {
            // Multiple owners found - display paginated list
            return setupPaginationModel(page, model, lastName, ownersResults);
        }
    }

    /**
     * Sets up the model attributes for pagination.
     */
    private String setupPaginationModel(int page, Model model, String lastName, Page<Owner> paginated) {
        List<Owner> listOwners = paginated.getContent();
        
        model.addAttribute("listOwners", listOwners);
        model.addAttribute("currentPage", page);
        model.addAttribute("totalPages", paginated.getTotalPages());
        model.addAttribute("totalItems", paginated.getTotalElements());
        
        return OWNERS_LIST_VIEW;
    }

    /**
     * Creates a paginated result of owners by last name.
     */
    private Page<Owner> findPaginatedByLastName(int page, String lastName) {
        Pageable pageable = PageRequest.of(page - 1, PAGE_SIZE);
        return ownerRepository.findByLastName(lastName, pageable);
    }

    /**
     * Displays the form for editing an existing owner.
     */
    @GetMapping("/owners/{ownerId}/edit")
    public String initUpdateOwnerForm(@PathVariable("ownerId") int ownerId, Model model) {
        Owner owner = this.ownerRepository.findById(ownerId);
        model.addAttribute(owner);
        return VIEWS_OWNER_CREATE_OR_UPDATE_FORM;
    }

    /**
     * Processes the form submission for updating an existing owner.
     */
    @PostMapping("/owners/{ownerId}/edit")
    public String processUpdateOwnerForm(@Valid Owner owner, BindingResult result,
                                       @PathVariable("ownerId") int ownerId) {
        if (result.hasErrors()) {
            return VIEWS_OWNER_CREATE_OR_UPDATE_FORM;
        }
        
        owner.setId(ownerId);
        this.ownerRepository.save(owner);
        return redirectToOwnerDetails(ownerId);
    }

    /**
     * Helper method to create the redirect URL to owner details.
     */
    private String redirectToOwnerDetails(int ownerId) {
        return "redirect:/owners/" + ownerId;
    }

    /**
     * Displays the details for a specific owner, including their pets and visits.
     */
    @GetMapping("/owners/{ownerId}")
    public ModelAndView showOwner(@PathVariable("ownerId") int ownerId) {
        ModelAndView modelAndView = new ModelAndView(OWNER_DETAILS_VIEW);
        Owner owner = this.ownerRepository.findById(ownerId);
        
        // Load visits for each pet
        for (Pet pet : owner.getPets()) {
            pet.setVisitsInternal(visitRepository.findByPetId(pet.getId()));
        }
        
        modelAndView.addObject(owner);
        return modelAndView;
    }
}